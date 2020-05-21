using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RaidCoordinator
{
    public class DiscordService
    {
        public DiscordSocketClient client { get; set; }
        public event ReadyChangeDelegate OnReadyChanged;

        public bool IsReady = false;

        private ILogger logger { get; }

        public DiscordService(ILogger<DiscordService> logger)
        {
            this.logger = logger;

            //this.MainAsync();
        }

        public async Task InitializeAsync(string discordToken)
        {
            logger.Log(LogLevel.Information, "Initializing..");

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
            });

            client.Ready += Client_Ready;
            client.Log += Client_Log;

            string token = discordToken;

            logger.Log(LogLevel.Information, $"Token: {token}");

            try
            {
                await client.LoginAsync(TokenType.Bot, token);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + e.StackTrace);
                throw;
            }

            await client.StartAsync();

            logger.Log(LogLevel.Information, "Finished Init..");

            //await Task.Delay(-1);
        }
       
        private async Task Client_Log(LogMessage Message)
        {
            logger.LogInformation($"{DateTime.Now} at {Message.Source} {Message.Message}");
        }

        private async Task Client_Ready()
        {
            try
            {
                await client.SetGameAsync("Raiding", "", ActivityType.Playing);

                this.OnReadyChanged(this, new ReadyChangeEventArgs(true));
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + e.StackTrace);
                throw;
            }
        }
    }
}
