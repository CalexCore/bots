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
        public void Process(SocketUserMessage msg)
        {
            var em = new EmbedBuilder()
            .WithAuthor("Amity Network Stats", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("Seed node status")
            .WithColor(Color.DarkRed)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            Request.ApiAll(AtomBotConfig.GetSeedNodes(), "daemon/get_info", msg.Channel, (rd) =>
            {

                foreach (var r in rd)
                {
                    string result = "No response...";

                    if (r.Value != null)
                    {
                        NodeInfo ni = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(r.Value.ResultString).Result;
                        result = 
                            $"Version: {ni.Version}\n" +
                            $"Height: {ni.Height}/{ni.TargetHeight}\n" +
                            $"Connections: {ni.IncomingConnections}/{ni.OutgoingConnections} in/out\n" +
                            $"Network Hashrate: {((ni.Difficulty / 60.0f) / 1000.0f)} kH/s\n" +
                            $"Top Block: {ni.TopBlockHash}";
                    }

                    em.AddField(r.Key, result);
                }

                DiscordResponse.Reply(msg, embed: em.Build());
            });
        }
    }
}