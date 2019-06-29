using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;

namespace Nerva.Bots.Commands
{
    [Command("ping", "Make sure the bot is alive")]
    public class Ping : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            DiscordResponse.Reply(msg, text: "Pong!");
        }
    }
}