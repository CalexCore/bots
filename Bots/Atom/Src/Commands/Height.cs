using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("height", "Get the current chain height")]
    public class Height : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            RequestData rd = await Request.Api(AtomBotConfig.SeedNodes, "daemon/get_block_count", msg.Channel);
            if (!rd.IsError)
            {
                ulong count = JsonConvert.DeserializeObject<JsonResult<GetBlockCount>>(rd.ResultString).Result.Count;
                await DiscordResponse.Reply(msg, text: $"Current height: {count}");
            }
        }
    }
}
