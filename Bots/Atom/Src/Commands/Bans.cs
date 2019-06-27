using System;
using System.Collections.Generic;
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
            
            //Use a HashSet here as the adding items is slower, but lookups to check for dups are 
            //orders of magnitude faster.
            HashSet<string> banList = new HashSet<string>();

            foreach (var sn in AtomBotConfig.SeedNodes)
                if (Request.Http($"{sn}api/getbans.php", out result))
                {
                    string[] split = result.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in split)
                        if (!banList.Contains(s))
                            banList.Add(s);
                }

            //Discord has a 2000 character message limit. It may be possible to exceed this if the ban list is large
            //So a mod to this code to split the ban list into smaller chunks may be appropriate, however the risk of 
            //exceeding the character limit is small, making this a job for another day
            StringBuilder sb = new StringBuilder();
            foreach (string s in banList)
                sb.AppendLine(s);

            DiscordResponse.Reply(msg, text: sb.ToString());
        }
    }
}