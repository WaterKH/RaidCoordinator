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
            await this.HandleRaidChannelTokenCommand();

            await ReplyAsync("Authentication token sent to your DMs.");
        }

        [Command("resetraidtoken"), Summary("Resets the token for the channel for party raiding. Use this if your token has been compromised.")]
        public async Task ResetRaidToken()
        {
            await this.HandleRaidChannelTokenCommand(true);

            await ReplyAsync("Authentication token reset! New authentication token sent to your DMs.");
        }

        private async Task HandleRaidChannelTokenCommand(bool resetToken = false)
        {
            var user = Context.User as SocketGuildUser;
            var role = user.Guild.Roles.FirstOrDefault(x => x.Name == "Raid Coordinator");

            if (user.Roles.Contains(role))
            {
                var messageDm = await user.GetOrCreateDMChannelAsync();
                try
                {
                    var embedMessage = await this.serviceProvider.GetRequiredService<RaidCoordinatorService>().SendChannelToken(Context.Channel.Id, resetToken);

                    await messageDm.SendMessageAsync("", false, embedMessage);
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
            }
        }
    }
}