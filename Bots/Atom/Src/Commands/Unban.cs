using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;

namespace Atom.Commands
{
    [Command("unban", "Get yourself unbanned from the seed nodes")]
    public class Unban : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            var matches = Regex.Matches(msg.Content.ToLower(), @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

            if (matches.Count == 0)
            {
                await DiscordResponse.Reply(msg, text: "Need an IP to unban!");
                return;
            }

            foreach (var m in matches)
            {
                bool partialFail = false;
                bool allFail = true;
                string ip = m.ToString();

                foreach (var s in AtomBotConfig.SeedNodes)
                {
                    RequestData rd = await Request.Http($"{s}/api/daemon/set_bans/?ip={ip}&ban=false&time=0");
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

                await DiscordResponse.Reply(msg, text: result);
            }
        }
    }
}