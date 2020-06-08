using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RaidCoordinator
{

    public class RaidModule : ModuleBase<SocketCommandContext>
    {
        private IServiceProvider serviceProvider;

        public RaidModule(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Command("setraid"), Summary("Sets the channel for party raiding and DMs the Raid Coordinator the channel Id and token.")]
        public async Task SetRaid()
        {
            var passed = await this.HandleRaidChannelTokenCommand();

            if (passed)
                await ReplyAsync("Authentication token sent to your DMs.");
        }

        [Command("resetraidtoken"), Summary("Resets the token for the channel for party raiding. Use this if your token has been compromised.")]
        public async Task ResetRaidToken()
        {
            var passed = await this.HandleRaidChannelTokenCommand(true);

            if(passed)
                await ReplyAsync("Authentication token reset! New authentication token sent to your DMs.");
        }

        [Command("resetraid"), Summary("Resets the channel for party raiding. Use this if you want to unlink the current channel.")]
        public async Task ResetRaid()
        {
            using var context = new RaidContext(new DbContextOptions<RaidContext>());

            context.ChannelTokens.Remove(context.ChannelTokens.FirstOrDefault(x => BitConverter.ToUInt64(x.ChannelId) == Context.Channel.Id));

            context.SaveChanges();

            await ReplyAsync("Channel removed. Please use the !setraid command to set a new channel.");
        }

        private async Task<bool> HandleRaidChannelTokenCommand(bool resetToken = false)
        {
            var user = Context.User as SocketGuildUser;
            var role = user.Guild.Roles.FirstOrDefault(x => x.Name == "Raid Coordinator");

            if (user.Roles.Contains(role))
            {
                var messageDm = await user.GetOrCreateDMChannelAsync();
                try
                {
                    var embedMessage = await this.serviceProvider.GetRequiredService<RaidCoordinatorService>().SendChannelToken(Context.Channel.Id, Context.Guild.Id, resetToken);

                    await messageDm.SendMessageAsync("", false, embedMessage);

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                await ReplyAsync("You don't have the appropriate role ~**Raid Coordinator**~ to set this channel.");

                return false;
            }
        }
    }
}