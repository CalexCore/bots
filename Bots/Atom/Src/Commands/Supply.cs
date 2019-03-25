using System;
using Atom;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("supply", "Get the current circulating supply")]
    public class Supply : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            string result;
            if (Request.Api("getgeneratedcoins", msg.Channel, out result))
            {
                ulong coins = JsonConvert.DeserializeObject<JsonResult<GetGeneratedCoins>>(result).Result.Coins;
                DiscordResponse.Reply(msg, text: $"Current Supply: {coins.FromAtomicUnits()}");
            }
        }
    }
}