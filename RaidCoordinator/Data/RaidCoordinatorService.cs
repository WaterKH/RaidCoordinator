using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RaidCoordinator.Data
{
    public class RaidCoordinatorService
    {
        protected IServiceProvider ServiceProvider { get; set; }
        protected DbContextOptions<RaidContext> DbContextOptions { get; set; }

        public bool IsReady = false;
        public Dictionary<ulong, RaidManager> ChannelManagerPair = new Dictionary<ulong, RaidManager>();
        
        private readonly Random Random = new Random();
        private ILogger logger;

        // Todo Add a custom emote option
        // todo Add more authentication than just sending channelid
        public RaidCoordinatorService(IServiceProvider serviceProvider, DbContextOptions<RaidContext> dbContextOptions, ILogger<RaidCoordinatorService> logger)
        {
            this.ServiceProvider = serviceProvider;
            this.DbContextOptions = dbContextOptions;
            this.logger = logger;
            
            this.ServiceProvider.GetRequiredService<DiscordService>().client.ReactionAdded += OnReactionChanged;
            this.ServiceProvider.GetRequiredService<DiscordService>().client.ReactionRemoved += OnReactionChanged;
            this.ServiceProvider.GetRequiredService<DiscordService>().client.ReactionsCleared += OnReactionsCleared;
            this.ServiceProvider.GetRequiredService<DiscordService>().client.MessageReceived += OnMessageReceived;

            this.logger.LogInformation("Func addition complete");

            using (var context = new RaidContext(DbContextOptions))
            {
                foreach (var channelToken in context.ChannelTokens)
                    this.ChannelManagerPair.Add(BitConverter.ToUInt64(channelToken.ChannelId), new RaidManager());
            }
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
                manager.UpdateBoostList(new BoostersAddedEventArgs(new Booster { Name = username, BoostedAt = DateTime.Now }));
            else if(this.CheckMessage(manager.TokenMessage, reaction.MessageId))
                manager.UpdateToken(this.Random.Next(256, Int32.MaxValue));
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
            var messageContent = message.Content.Split(":");

            if (ulong.TryParse(messageContent[0], out ulong channelId))
            {
                try
                {
                    using (var context = new RaidContext(DbContextOptions))
                    {
                        ChannelToken channelTokenObject = null;
                        foreach (var channelToken in context.ChannelTokens)
                        {
                            if (BitConverter.ToUInt64(channelToken.ChannelId) == channelId)
                            {
                                channelTokenObject = channelToken;
                                break;
                            }
                        }

                        if (channelTokenObject == null)
                        {
                            var token = this.Random.Next(256, Int32.MaxValue);

                            context.Add(new ChannelToken {ChannelId = BitConverter.GetBytes(channelId), Token = token});

                            context.SaveChanges();

                            await message.Channel.SendMessageAsync($"Use this token to authenticate when you put your Channel Id in: {token}");
                        }
                        else
                        {
                            if (messageContent.Length > 1 && messageContent[1].ToLower().Equals("new token"))
                            {
                                channelTokenObject.Token = this.Random.Next(256, Int32.MaxValue);

                                context.SaveChanges();

                                await message.Channel.SendMessageAsync($"Use this token to authenticate when you put your Channel Id in: {channelTokenObject.Token}");
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("Please use the last token generated. If you would like a new token, message in this format: \"<channelId>:New Token\"");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + e.StackTrace);
                    throw;
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("I'm sorry, please just paste your Channel Id in your message");
            }
        }
    }
}
