using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace RaidCoordinator
{
    public class DiscordService
    {
        public DiscordSocketClient client { get; set; }
        public event ReadyChangeDelegate OnReadyChanged;

        public IConfiguration Configuration { get; }

        //private static DiscordManager instance = new DiscordManager();

        //public static DiscordManager Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //            instance = new DiscordManager();
                
        //        return instance;
        //    }
        //}

        public DiscordService(IConfiguration configuration)
        {
            this.Configuration = configuration;

            this.MainAsync();
        }

        private async Task MainAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
            });

            client.Ready += Client_Ready;
            client.Log += Client_Log;

            string token = Configuration["DiscordToken"];

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
       
        private async Task Client_Log(LogMessage Message)
        {
            Console.WriteLine($"{DateTime.Now} at {Message.Source} {Message.Message}");
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
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
