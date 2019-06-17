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
        public ulong OwnerID => 388852986316587010;
        public ulong ServerID => 479322729007546368;
        //FORK: Set the correct bot id and remove this comment
        public ulong BotID => 450609948246671360;
        //FORK: Set the correct bot channel id and remove this comment
        public ulong BotChannelID => 450660331405049876;
        public string CmdPrefix => "!";

        //FORK: set all the available seed nodes that have an api and remove this comment
        //Should be accessible via <url>/api/xxx.php
        public static readonly string[] SeedNodes = new string[]
        {
            "https://xnv1.getnerva.org",
            "https://xnv2.getnerva.org"
        };
    }

    class AtomBot : IBot
    {
        private AtomBotConfig cfg = new AtomBotConfig();

        public void Init(CommandLineParser cmd)
        {
        }

        public Task ClientReady()
        {
            return Task.CompletedTask;
        }

        public IBotConfig Config => cfg;
    }
}
