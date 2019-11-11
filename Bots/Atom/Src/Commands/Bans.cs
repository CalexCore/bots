using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("bans", "Get a list of banned ip addresses")]
    public class Bans : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            //Use a HashSet here as the adding items is slower, but lookups to check for dups are 
            //orders of magnitude faster.
            HashSet<string> banList = new HashSet<string>();

            foreach (var sn in AtomBotConfig.SeedNodes)
            {
                RequestData rd = await Request.Http($"{sn}/api/daemon/get_bans/");
                if (!string.IsNullOrEmpty(rd.ResultString))
                {
                    List<BanListItem> bl = JsonConvert.DeserializeObject<JsonResult<BanList>>(rd.ResultString).Result.Bans;

                    foreach (var b in bl)
                        if (!banList.Contains(b.Host))
                            banList.Add(b.Host);
                }
            }

            //Discord has a 2000 character message limit. It may be possible to exceed this if the ban list is large
            //So a mod to this code to split the ban list into smaller chunks may be appropriate, however the risk of 
            //exceeding the character limit is small, making this a job for another day
            StringBuilder sb = new StringBuilder();
            foreach (string s in banList)
                sb.AppendLine(s);

            await DiscordResponse.Reply(msg, text: sb.ToString());
        }
    }
}