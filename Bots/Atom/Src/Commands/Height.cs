using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("height", "Get the current chain height")]
    public class Height : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            string result;
            if (Request.Api(AtomBotConfig.SeedNodes, "getblockcount", msg.Channel, out result))
            {
                ulong count = JsonConvert.DeserializeObject<JsonResult<GetBlockCount>>(result).Result.Count;
                DiscordResponse.Reply(msg, text: $"Current height: {count}");
            }
        }
    }
}
