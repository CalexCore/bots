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
