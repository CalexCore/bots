using System;
using System.IO;
using System.Text;
using Atom;
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
            NodeInfo seed1 = null;
            NodeInfo seed2 = null;
            NodeInfo seed3 = null;

            string result;
            if (Request.Http($"https://xnv1.getnerva.org/api/getinfo.php", out result))
                seed1 = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(result).Result;

            if (Request.Http($"https://xnv2.getnerva.org/api/getinfo.php", out result))
                seed2 = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(result).Result;

            if (Request.Http($"https://xnv3.getnerva.org/api/getinfo.php", out result))
                seed3 = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(result).Result;

            var em = new EmbedBuilder()
            .WithAuthor("Seed Node Information", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("The latest seed node status")
            .WithColor(Color.DarkMagenta)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            string s1 = GetSeedInfoString(seed1);
            string s2 = GetSeedInfoString(seed2);
            string s3 = GetSeedInfoString(seed3);

            em.AddField("XNV-1", s1, true);
            em.AddField("XNV-2", s2, true);
            em.AddField("XNV-3", s3, true);

            DiscordResponse.Reply(msg, embed: em.Build());
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