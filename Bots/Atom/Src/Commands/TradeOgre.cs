using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("tradeogre", "Get market info from TradeOgre")]
    public class TradeOgre : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            RequestData rd = await Request.Http("https://tradeogre.com/api/v1/ticker/btc-xnv");
            if (!rd.IsError)
            {
                var json = JsonConvert.DeserializeObject<MarketInfo>(rd.ResultString);

                var em = new EmbedBuilder()
                .WithAuthor("TradeOgre Details", Globals.Client.CurrentUser.GetAvatarUrl())
                .WithDescription("The latest pricing from TradeOgre")
                .WithColor(Color.DarkGreen)
                .WithThumbnailUrl("https://getnerva.org/content/images/tradeogre-logo.png");

                em.AddField("Volume", Math.Round(json.Volume, 5));
                em.AddField("Buy", json.Ask * 100000000.0d, true);
                em.AddField("Sell", json.Bid * 100000000.0d, true);
                em.AddField("High", json.High * 100000000.0d, true);
                em.AddField("Low", json.Low * 100000000.0d, true);

                await DiscordResponse.Reply(msg, embed: em.Build());
            }
        }
    }
}