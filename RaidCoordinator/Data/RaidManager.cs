using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RaidCoordinator
{
    public class RaidManager
    {
        public event RaidersChangeDelegate OnRaidersChanged;
        public event BoostersAddedDelegate OnBoostersAdded;

        public IUserMessage RaidRequestMessage;
        public IUserMessage CurrentRaidMessage;
        public IUserMessage BoostMessage;
        public IUserMessage TokenMessage;

        public IMessageChannel DiscordChannel;
        public Channel Channel;

        public List<Raider> Raiders = new List<Raider>();
        public List<string> Boosters = new List<string>();
        public int NumberOfIterations = 0;

        public bool IsValidated = false;
        public RaidBonusTime RaidBonusTime;

        //private ILogger logger;

        public RaidManager()
        {
            //this.logger.LogInformation("RaidManager Init");

            this.Channel = new Channel();
            this.NumberOfIterations = 0;
        }

        #region Discord Function Events

        public void UpdateOnRaidersChangedEvent(RaidersChangeEventArgs eventArgs)
        {
            this.OnRaidersChanged(this, eventArgs);
        }

        public void UpdateBoostList(BoostersAddedEventArgs eventArgs)
        {
            this.OnBoostersAdded(this, eventArgs);
        }

        public void UpdateToken(int token)
        {
            this.Channel.Token = token;
        }

        public void UpdateRaidBonusTime(RaidBonusTime bonusTime)
        {
            this.RaidBonusTime = bonusTime;


        }

        #endregion

        #region Channel/ Token Authentication

        public async Task<bool> ValidateRaidServiceAndChannel(IMessageChannel channel)
        {
            if (channel != null)
            {
                this.DiscordChannel = channel;

                this.IsValidated = true;

                return true;
            }
            else
            {
                this.DiscordChannel = null;

                this.IsValidated = false;

                return false;
            }
        }

        #endregion

        #region Send Message Methods

        public async Task SendRaidRequestMessage()
        {
            try
            {
                if (this.RaidRequestMessage != null)
                    await this.RaidRequestMessage.ModifyAsync(message => message.Content = "React to the latest raid request to be added to the queue.");

                this.RaidRequestMessage = await this.DiscordChannel.SendMessageAsync("Raid Boss Raider Request - React to this message to join the ranks!");
                await this.RaidRequestMessage.AddReactionAsync(new Emoji("\uD83D\uDC62"));
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message + e.StackTrace);
                throw;
            }
        }

        public async Task SendBoostRequestMessage()
        {
            try
            {
                this.BoostMessage = await this.DiscordChannel.SendMessageAsync("@everyone BOOST TIME - React to this message to let us know you've boosted");
                await this.BoostMessage.AddReactionAsync(new Emoji("\u2757"));
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message + e.StackTrace);
                throw;
            }
        }

        public async Task SendSpawnRequestMessage(string user)
        {
            try
            {
                this.CurrentRaidMessage = await this.DiscordChannel.SendMessageAsync($"Attack {user}'s raid boss!");

                this.Raiders.FirstOrDefault(x => x.Username == user).HasSpawnedBoss = true;
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message + e.StackTrace);
                throw;
            }
        }

        public async Task SendJoinRequestMessage()
        {
            try
            {
                await this.CurrentRaidMessage.AddReactionAsync(new Emoji("\u2705"));
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message + e.StackTrace);
                throw;
            }
        }

        public async Task<List<Raider>> SendKilledRequestMessage(Raider user, List<Raider> raiders)
        {
            // Remove the user and add it to the back of the list
            raiders.Remove(user);
            raiders.Add(user);

            try
            {
                await this.CurrentRaidMessage.AddReactionAsync(new Emoji("\uD83D\uDCA5"));
                
                user.IsAvailable = false;
                user.HasSpawnedBoss = false;
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message + e.StackTrace);
                throw;
            }

            return raiders;
        }

        #endregion

        public async Task<List<Raider>> ResetRaiders(List<Raider> raiders)
        {
            foreach (var raider in raiders)
            {
                raider.IsAvailable = true;
                raider.HasSpawnedBoss = false;
            }

            this.CurrentRaidMessage = null;

            try
            {
                /*this.RaidRequestMessage =*/ await this.DiscordChannel.SendMessageAsync("Resummon raid bosses!");
            }
            catch (Exception e)
            {
                //logger.LogError(e.Message + e.StackTrace);
                throw;
            }

            ++NumberOfIterations;

            return raiders;
        }

        public async Task FinishRaiding()
        {
            var raiders = string.Join(", ", this.Raiders.Select(x => x.Username));
            var boosters = string.Join(", ", this.Boosters);

            if (string.IsNullOrEmpty(raiders) && string.IsNullOrEmpty(boosters) && this.NumberOfIterations == 0)
                return;

            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Raid Coordinating Complete");

            builder.AddField("Number of iterations: ", this.NumberOfIterations);
            builder.AddField("Raiders: ", string.IsNullOrEmpty(raiders) ? "null" : raiders);
            builder.AddField("Boosters: ", string.IsNullOrEmpty(boosters) ? "null" : boosters);

            await this.DiscordChannel.SendMessageAsync("", false, builder.Build());

            this.Clear();

            this.UpdateBoostList(null);
        }

        public void Clear()
        {
            this.Raiders.Clear();
            this.Boosters.Clear();

            this.NumberOfIterations = 0;
            this.Channel = new Channel();
            this.DiscordChannel = null;
            this.BoostMessage = null;
            this.RaidRequestMessage = null;
            this.CurrentRaidMessage = null;
        }
    }
}
