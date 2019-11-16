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
    [Command("links", "Get official Amity download links")]
    public class Links : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            RequestData rd = await Request.Http("https://getamitycoin.org/getbinaries.php");
            if (!rd.IsError)
            {
                var json = JsonConvert.DeserializeObject<LinkData>(rd.ResultString);

                var em = new EmbedBuilder()
                .WithAuthor("Download Links", Globals.Client.CurrentUser.GetAvatarUrl())
                .WithDescription($"Current CLI: {json.CliVersion}")
                .WithColor(Color.DarkPurple)
                .WithThumbnailUrl("https://getamitycoin.org/assets/amity-logo.png");

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"Windows: [CLI]({json.BinaryUrl}assets/{json.WindowsLink}) | [CLI]({json.BinaryUrl}assets/{json.ArmhfLink})");
                sb.AppendLine($"Linux: [CLI]({json.BinaryUrl}assets/{json.LinuxLink}) | [CLI]({json.BinaryUrl}assets/{json.Aarch64Link})");
                sb.AppendLine($"MacOS: [CLI]({json.BinaryUrl}assets/{json.MacLink}) | [CLI]({json.BinaryUrl}assets/{json.Riscv64Link})");

                em.AddField($"Nerva Tools", sb.ToString());

                sb = new StringBuilder();
                sb.AppendLine($"[QuickSync]({json.BinaryUrl}assets/quicksync.raw)");

                em.AddField($"Chain Data", sb.ToString());

                await DiscordResponse.Reply(msg, embed: em.Build());
            }
        }
    }
}