using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using AngryWasp.Helpers;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using Nerva.Bots.Plugin;
using Nerva.Bots.Helpers;
using Nerva.Bots;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace Atom
{
    public class AtomBotConfig : IBotConfig
    {
        public ulong OwnerID => 407511685134549003;
        public ulong ServerID => 439649936414474256;
        public ulong BotID => 450609948246671360;
        public ulong BotChannelID => 450660331405049876;
        public string CmdPrefix => "!";
    }

    class AtomBot : IBot
    {
        private AtomBotConfig cfg = new AtomBotConfig();

        public void Init(CommandLineParser cmd)
        {
            string iPath = Path.Combine(Environment.CurrentDirectory, "tile");

            if (!File.Exists(iPath))
                File.WriteAllText(iPath, "0");

            Timer timer = new Timer(14400000);
            timer.Elapsed += (s, e) => PostTiles();
            timer.Start();
        }

        public Task ClientReady()
        {
            PostTiles();
            return Task.CompletedTask;
        }

        public IBotConfig Config => cfg;

        private void PostTiles()
        {
            /*Task.Run(() =>
            {
                string url = FactTiles.GetImage();
                foreach (ulong server in FactTiles.SERVERS)
                {
                    var channel = Globals.Client.GetChannel(server) as IMessageChannel;
                    var msgs = channel.GetMessagesAsync(100).FlattenAsync().Result.ToList();

                    List<ulong> oldMsgs = new List<ulong>();

                    foreach (var m in msgs)
                        if (m.Author.Id == cfg.BotID)
                            oldMsgs.Add(m.Id);

                    if (oldMsgs.Count > 0)
                        channel.DeleteMessagesAsync(oldMsgs);

                    channel.SendMessageAsync(url);
                }
            });*/
        }
    }
}
