using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace RaidCoordinator.Data
{
    public class RaidCoordinatorService
    {
        protected IServiceProvider ServiceProvider { get; set; }
        protected DbContextOptions<RaidContext> DbContextOptions { get; set; }

        public bool IsReady = false;
        //public Dictionary<ulong, int> ChannelTokenPair = new Dictionary<ulong, int>();
        public Dictionary<ulong, RaidManager> ChannelManagerPair = new Dictionary<ulong, RaidManager>();
        
        private readonly Random Random = new Random();

        // Todo Add id as an option to fill in as well as a custom emote
        // Todo Add channel as a variable so we aren't polling for it each time
        public RaidCoordinatorService(IServiceProvider serviceProvider, DbContextOptions<RaidContext> dbContextOptions)
        {
            this.ServiceProvider = serviceProvider;
            this.DbContextOptions = dbContextOptions;

            this.ServiceProvider.GetRequiredService<DiscordService>().client.ReactionAdded += OnReactionChanged;
            this.ServiceProvider.GetRequiredService<DiscordService>().client.ReactionRemoved += OnReactionChanged;
            this.ServiceProvider.GetRequiredService<DiscordService>().client.ReactionsCleared += OnReactionsCleared;
            this.ServiceProvider.GetRequiredService<DiscordService>().client.MessageReceived += OnMessageReceived;
        }

        private bool CheckMessage(IMessage message, ulong messageIdToCheck)
        {
            return message != null && messageIdToCheck == message.Id;
        }

        public async Task OnReactionChanged(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var manager = this.ChannelManagerPair[channel.Id];

            if (reaction.User.Value.IsBot || manager == null)
                return;

            var username = reaction.User.Value.Username;

            if(this.CheckMessage(manager.RaidRequestMessage, reaction.MessageId))
                manager.UpdateOnRaidersChangedEvent(new RaidersChangeEventArgs(new Raider { Username = username, IsAvailable = true }));
            else if (this.CheckMessage(manager.BoostMessage, reaction.MessageId))
                manager.UpdateBoostList(new BoostersAddedEventArgs(username));
        }

        public async Task OnReactionsCleared(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel)
        {
            var manager = this.ChannelManagerPair[channel.Id];

            if (manager == null || cache.Id != manager.RaidRequestMessage.Id)
                return;

            manager.UpdateOnRaidersChangedEvent(null);
        }

        public async Task OnMessageReceived(SocketMessage message)
        {
            if (!(message.Channel is SocketDMChannel) || message.Author.IsBot)
                return;

            await SendChannelToken(message);
        }

        public async Task SendChannelToken(SocketMessage message)
        {
            if (ulong.TryParse(message.Content, out ulong result))
            {
                var token = this.Random.Next(256, Int32.MaxValue);

                // Todo send token and result to a database to keep track of
                //if(!this.ChannelTokenPair.ContainsKey(result))
                //    ChannelTokenPair.Add(result, 0);

                //ChannelTokenPair[result] = token;
                try
                {
                    using (var context = new RaidContext(DbContextOptions))
                    {
                        ChannelToken channelTokenObject = null;
                        foreach (var channelToken in context.ChannelTokens)
                        {
                            if (BitConverter.ToUInt64(channelToken.ChannelId) == result)
                            {
                                channelTokenObject = channelToken;
                                break;
                            }
                        }

                        if (channelTokenObject == null)
                        {
                            context.Add(new ChannelToken { ChannelId = BitConverter.GetBytes(result), Token = token });
                        }
                        else
                        {
                            channelTokenObject.Token = token;
                        }

                        context.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                await message.Channel.SendMessageAsync($"Use this token to authenticate when you put your Channel Id in: {token}");
            }
            else
            {
                await message.Channel.SendMessageAsync("I'm sorry, please just paste your Channel Id in your message");
            }
        }
    }
}
