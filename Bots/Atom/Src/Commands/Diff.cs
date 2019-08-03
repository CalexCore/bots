using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("diff", "Get the current network difficulty")]
    public class Diff : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            RequestData rd = await Request.Api(AtomBotConfig.SeedNodes, "daemon/get_info", msg.Channel);
            if (!rd.IsError)
            {
                int diff = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(rd.ResultString).Result.Difficulty;
                await DiscordResponse.Reply(msg, text: $"Current difficulty: {diff}");
            }
        }
    }
}
