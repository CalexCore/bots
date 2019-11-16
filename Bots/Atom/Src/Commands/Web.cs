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
            .WithDescription("Need more Amity information?")
            .WithColor(Color.DarkGrey)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            em.AddField("Website", "[getamitycoin.org](https://getamitycoin.org/)", true);
            em.AddField("Twitter", "[@AmityCore](https://twitter.com/AmityCore)", true);
            em.AddField("Reddit", "[r/AmityCoin](https://www.reddit.com/r/AmityCoin)", true);
            em.AddField("Source Code", "[Gitlab](https://gitlab.org/amity-project)", true);
            em.AddField("Block Explorer", "[explorer.getamitycoin.org](https://explorer.getamitycoin.org/)", true);

            await DiscordResponse.Reply(msg, embed: em.Build());
        }
    }
}