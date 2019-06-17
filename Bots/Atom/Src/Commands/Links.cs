using System.Text;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("links", "Get official Amity releases")]
    public class Links : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            DiscordResponse.Reply(msg, text: "https://github.com/CalexCore/AmityCoinV3/releases");
        }
    }
}