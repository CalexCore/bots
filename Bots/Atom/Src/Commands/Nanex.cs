using System;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("nanex", "Get market info from Nanex")]
    public class Nanex : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            string btc = null, nano = null;
            Request.Http("https://nanex.co/api/public/ticker/xnvbtc", out btc);
            Request.Http("https://nanex.co/api/public/ticker/xnvnano", out nano);

            var em = new EmbedBuilder()
            .WithAuthor("Nanex Details", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("The latest pricing from Nanex")
            .WithColor(Color.DarkBlue)
            .WithThumbnailUrl("https://getnerva.org/content/images/nanex-logo.png");

            if (btc != null)
            {
                var json = JsonConvert.DeserializeObject<NanexInfo>(btc);

                string change = json.PriceChange.ToString() + "%";
                if (json.PriceChange > 0)
                    change = "+" + change;
                em.AddField("BTC", "----------");
                em.AddField("Change", change, true);
                em.AddField("Last", $"{(json.LastTrade * 100000000.0d)} sats", true);
                em.AddField("XNV Vol.", $"{Math.Round(json.BaseVolume, 3)} xnv", true);
                em.AddField("BTC Vol.", $"{Math.Round(json.QuoteVolume, 3)} btc \u200b", true);
            }
            else
                em.AddField("BTC", "No data available \u200b");

            if (nano != null)
            {
                var json = JsonConvert.DeserializeObject<NanexInfo>(nano);

                string change = json.PriceChange.ToString() + "%";
                if (json.PriceChange > 0)
                    change = "+" + change;

                em.AddField("NANO", "----------");
                em.AddField("Change", change, true);
                em.AddField("Last", $"{Math.Round(json.LastTrade, 4)} nano", true);
                em.AddField("XNV Vol.", $"{Math.Round(json.BaseVolume, 3)} xnv", true);
                em.AddField("NANO Vol.", $"{Math.Round(json.QuoteVolume, 3)} nano", true);
            }
            else
                em.AddField("NANO", "No data available");

            DiscordResponse.Reply(msg, embed: em.Build());
        }
    }
}