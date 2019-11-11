using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("coingecko", "Get info from CoinGecko")]
    public class CoinGecko : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            RequestData rd = await Request.Http("https://api.coingecko.com/api/v3/coins/nerva?localization=false");
            if (!rd.IsError)
            {
                var json = JsonConvert.DeserializeObject<CoinGeckoInfo>(rd.ResultString);

                var em = new EmbedBuilder()
                .WithAuthor("CoinGecko Details", Globals.Client.CurrentUser.GetAvatarUrl())
                .WithDescription("The latest scores and rankings from CoinGecko")
                .WithColor(Color.DarkTeal)
                .WithThumbnailUrl("https://getnerva.org/content/images/coingecko-logo.png");

                em.AddField("CoinGecko Rank", json.CoinGeckoRank, true);
                em.AddField("CoinGecko Score", json.CoinGeckoScore, true);
                em.AddField("Market Cap Rank", json.MarketCapRank, true);
                em.AddField("Community Score", json.CommunityScore, true);
                em.AddField("Developer Score", json.DeveloperScore, true);
                em.AddField("Public Interest Score", json.PublicInterestScore, true);

                await DiscordResponse.Reply(msg, embed: em.Build());
            }
        }
    }
}