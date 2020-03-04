using System.Text.RegularExpressions;
using Discord.WebSocket;
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
                bool partialFail = false;
                bool allFail = true;
                string ip = m.ToString();

                foreach (var s in AtomBotConfig.GetSeedNodes())
                {
                    RequestData rd = Request.Http($"http://{s}/api/daemon/set_bans/?ip={ip}&ban=false&time=0");
                    if (!rd.IsError)
                        allFail = false;
                    else
                        partialFail = true;
                }

                string result = null;

                if (allFail)
                    result = $"An error occurred attempting to unban IP {ip}";
                else if (partialFail)
                    result = $"IP {ip} could not be unbanned from all seed nodes";
                else
                    result = $"IP {ip} unbanned successfully";

                DiscordResponse.Reply(msg, text: result);
            }
        }
    }
}