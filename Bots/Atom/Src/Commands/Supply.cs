using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

#pragma warning disable 4014

namespace Atom.Commands
{
    [Command("supply", "Get the current circulating supply")]
    public class Supply : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            RequestData rd = Request.ApiAny(AtomBotConfig.GetSeedNodes(), "daemon/get_generated_coins", msg.Channel);
            if (!rd.IsError)
            {
                ulong coins = JsonConvert.DeserializeObject<JsonResult<GetGeneratedCoins>>(rd.ResultString).Result.Coins;
                DiscordResponse.Reply(msg, text: $"Current Supply: {coins.FromAtomicUnits()}");
            }
        }
    }
}