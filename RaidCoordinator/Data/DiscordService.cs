﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace RaidCoordinator
{
    public class DiscordService
    {
        public DiscordSocketClient client { get; set; }
        protected CommandService commands;
        protected IServiceProvider services;

        public event ReadyChangeDelegate OnReadyChanged;

        public bool IsReady = false;

        private ILogger logger { get; }

        public DiscordService(ILogger<DiscordService> logger, IServiceProvider services)
        {
            this.logger = logger;
            this.services = services;

            this.commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
            });
            logger.LogInformation("Commands Config Set");
        }

        public async Task InitializeAsync(string discordToken)
        {
            logger.Log(LogLevel.Information, "Initializing..");

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
            });

            await this.InstallCommands();

            client.Ready += Client_Ready;
            client.Log += Client_Log;

            try
            {
                await client.LoginAsync(TokenType.Bot, discordToken);
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

        private async Task InstallCommands()
        {
            logger.LogInformation("Installing Commands");
            
            this.client.MessageReceived += HandleCommand;
            
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            
            logger.LogInformation("Commands Installed");
        }

        private async Task HandleCommand(SocketMessage messageParam)
        {
            if (messageParam.Author.IsBot || messageParam.Channel is SocketDMChannel)
                return;

            var message = messageParam as SocketUserMessage;
            if (message == null) 
                return;

            int argPos = 0;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!message.HasCharPrefix('!', ref argPos)) 
                return;

            // Create a Command Context
            var context = new SocketCommandContext(client, message);

            logger.LogInformation("Send Command");

            await commands.ExecuteAsync(context, argPos, services);

            logger.LogInformation("Command Sent");
        }
    }
}
