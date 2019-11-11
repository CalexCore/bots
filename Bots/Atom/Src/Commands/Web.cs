using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;

namespace Atom.Commands
{
    [Command("web", "Get some useful web links")]
    public class Web : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            var em = new EmbedBuilder()
            .WithAuthor("Web Links", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("Need more NERVA information?")
            .WithColor(Color.DarkGrey)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            em.AddField("Website", "[getnerva.org](https://getnerva.org/)", true);
            em.AddField("Twitter", "[@NervaCurrency](https://twitter.com/NervaCurrency)", true);
            em.AddField("Reddit", "[r/Nerva](https://www.reddit.com/r/Nerva)", true);
            em.AddField("Source Code", "[BitBucket](https://bitbucket.org/nerva-project)", true);
            em.AddField("NERVA Stats", "[FreeBoard](https://freeboard.io/board/EV5-se)", true);
            em.AddField("Block Explorer", "[explorer.getnerva.org](https://explorer.getnerva.org/)", true);
            em.AddField("CPU Benchmarks", "[Forkmaps.com](https://forkmaps.com/#/benchmarks)", true);
            em.AddField("Public Node hosted by Hooftly", "[pubnodes.com](https://www.pubnodes.com/) | [Explorer](https://xnvex.pubnodes.com)");

            await DiscordResponse.Reply(msg, embed: em.Build());
        }
    }
}