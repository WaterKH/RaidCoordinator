using Discord.WebSocket;

namespace RaidCoordinator
{
    public interface IDiscordService
    {
        DiscordSocketClient client { get; set; }
        event ReadyChangeDelegate OnReadyChanged;


    }
}
