using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("seeds", "Get seed node info")]
    public class Seeds : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            var em = new EmbedBuilder()
            .WithAuthor("Seed Node Information", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("The latest seed node status")
            .WithColor(Color.DarkMagenta)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            int x = 0;
            foreach (var s in AtomBotConfig.SeedNodes)
            {
                ++x;

                try
                {
                    NodeInfo ni = null;
                    RequestData rd = await Request.Http($"{s}/api/daemon/get_info/");
                    if (!rd.IsError)
                        ni = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(rd.ResultString).Result;

                    em.AddField($"Seed {x}", GetSeedInfoString(ni), true);
                }
                catch
                {
                    em.AddField($"Seed {x}", "Not Available", true);
                    continue;
                }
                
            }

            await DiscordResponse.Reply(msg, embed: em.Build());
        }

        private string GetSeedInfoString(NodeInfo s)
        {
            string st;
            if (s != null)
            {
                st = $"Version: {s.Version}\n" +
                    $"Height: {s.Height}/{s.TargetHeight}\n" +
                    $"Connections: {s.IncomingConnections}/{s.OutgoingConnections} in/out\n" +
                    $"Network Hashrate: {((s.Difficulty / 60.0f) / 1000.0f)} kH/s\n" +
                    $"Top Block: {s.TopBlockHash}";
            }
            else
                st = "Unreachable";

            return st;
        }
    }
}