using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("supply", "Get the current circulating supply")]
    public class Supply : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            RequestData rd = await Request.Api(AtomBotConfig.SeedNodes, "daemon/get_generated_coins", msg.Channel);
            if (!rd.IsError)
            {
                ulong coins = JsonConvert.DeserializeObject<JsonResult<GetGeneratedCoins>>(rd.ResultString).Result.Coins;
                await DiscordResponse.Reply(msg, text: $"Current Supply: {coins.FromAtomicUnits()}");
            }
        }
    }
}