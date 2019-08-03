using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("links", "Get official Nerva download links")]
    public class Links : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            RequestData rd = await Request.Http("https://getnerva.org/getbinaries.php");
            if (!rd.IsError)
            {
                var json = JsonConvert.DeserializeObject<LinkData>(rd.ResultString);

                var em = new EmbedBuilder()
                .WithAuthor("Download Links", Globals.Client.CurrentUser.GetAvatarUrl())
                .WithDescription($"Current CLI: {json.CliVersion}\nCurrent GUI: {json.GuiVersion}")
                .WithColor(Color.DarkPurple)
                .WithThumbnailUrl("https://getnerva.org/content/images/dropbox-logo.png");

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"Windows: [CLI]({json.BinaryUrl}binaries/{json.WindowsLink}) | [GUI]({json.BinaryUrl}binaries/{json.WindowsGuiLink})");
                sb.AppendLine($"Linux: [CLI]({json.BinaryUrl}binaries/{json.LinuxLink}) | [GUI]({json.BinaryUrl}binaries/{json.LinuxGuiLink})");
                sb.AppendLine($"MacOS: [CLI]({json.BinaryUrl}binaries/{json.MacLink}) | [GUI]({json.BinaryUrl}binaries/{json.MacGuiLink})");

                em.AddField($"Nerva Tools", sb.ToString());

                sb = new StringBuilder();
                sb.AppendLine($"[Bootstrap]({json.BinaryUrl}bootstrap/mainnet.raw) | [QuickSync]({json.BinaryUrl}bootstrap/quicksync.raw)");

                em.AddField($"Chain Data", sb.ToString());

                await DiscordResponse.Reply(msg, embed: em.Build());
            }
        }
    }
}