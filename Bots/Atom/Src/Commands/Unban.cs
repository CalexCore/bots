using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;

namespace Atom.Commands
{
    [Command("unban", "Get yourself unbanned from the seed nodes")]
    public class Unban : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            var matches = Regex.Matches(msg.Content.ToLower(), @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

            if (matches.Count == 0)
            {
                DiscordResponse.Reply(msg, text: "Need an IP to unban!");
                return;
            }
            foreach (var m in matches)
            {
                string m2;
                string ip = m.ToString();
                string result;
                if (!Request.Http($"https://xnv1.getnerva.org/api/setbans.php?ip={ip}&ban=false&time=0", out result) ||
                    !Request.Http($"https://xnv2.getnerva.org/api/setbans.php?ip={ip}&ban=false&time=0", out result))
                    m2 = $"Sorry, couldn't totally unban IP {ip}";
                else
                    m2 = $"{ip} has been unbanned";

                DiscordResponse.Reply(msg, text: m2);
            }
        }
    }
}