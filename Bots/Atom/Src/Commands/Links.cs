using System.Text;
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
        public void Process(SocketUserMessage msg)
        {
            Request.Http("https://cdn.getamitycoin.org/getbinaries.php", (rd) =>
            {
				if (!rd.IsError)
				{
					var json = JsonConvert.DeserializeObject<LinkData>(rd.ResultString);

					var em = new EmbedBuilder()
					.WithAuthor("Download Links", Globals.Client.CurrentUser.GetAvatarUrl())
					.WithDescription($"Current CLI: {json.CliVersion}")
					.WithColor(Color.DarkPurple)
					.WithThumbnailUrl("https://getamitycoin.org/assets/amity-logo.png");

					StringBuilder sb = new StringBuilder();

					sb.AppendLine($"Windows: [CLI]({json.BinaryUrl}{json.WindowsLink})");
					sb.AppendLine($"Linux: [CLI]({json.BinaryUrl}{json.LinuxLink})");
					sb.AppendLine($"MacOS: [CLI]({json.BinaryUrl}{json.MacLink})");
					sb.AppendLine($"ARMHF: [CLI]({json.BinaryUrl}{json.ArmhfLink})");
					sb.AppendLine($"AARCH64: [CLI]({json.BinaryUrl}{json.Aarch64Link})");
					sb.AppendLine($"RISCV64: [CLI]({json.BinaryUrl}{json.Riscv64Link})");

					em.AddField($"Amity Tools", sb.ToString());

					sb = new StringBuilder();
					sb.AppendLine($"[Bootstrap]({json.BinaryUrl}mainnet.raw) | [QuickSync]({json.BinaryUrl}quicksync.raw)");

					em.AddField($"Chain Data", sb.ToString());

					DiscordResponse.Reply(msg, embed: em.Build());
				}
            });
        }
    }
}
