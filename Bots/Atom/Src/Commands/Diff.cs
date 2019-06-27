using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("diff", "Get the current network difficulty")]
    public class Diff : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            string result;
            if (Request.Api(AtomBotConfig.SeedNodes, "getinfo", msg.Channel, out result))
            {
                int diff = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(result).Result.Difficulty;
                DiscordResponse.Reply(msg, text: $"Current difficulty: {diff}");
            }
        }
    }
}
