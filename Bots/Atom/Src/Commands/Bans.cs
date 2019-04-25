using System;
using System.IO;
using System.Text;
using Atom;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("bans", "Get a list of banned ip addresses")]
    public class Bans : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            string result = string.Empty;
            string temp;

            if (Request.Http($"https://xnv1.getnerva.org/api/getbans.php", out temp))
                result += temp;

            if (Request.Http($"https://xnv2.getnerva.org/api/getbans.php", out temp))
                result += temp;

            string[] split = result.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();

            foreach (string s in split)
                sb.AppendLine(s);

            File.WriteAllText("/var/www/html/banlist.txt", sb.ToString());

            DiscordResponse.Reply(msg, text: "https://xnv1.getnerva.org/banlist.txt");
        }
    }
}