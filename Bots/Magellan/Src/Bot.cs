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

namespace Magellan
{
    public class MagellanBotConfig : IBotConfig
    {
        public ulong OwnerID => 407511685134549003;
        public ulong ServerID => 439649936414474256;
        public ulong BotID => 557573274259816450;
        public ulong BotChannelID => 558899021796868096;
        public string CmdPrefix => ".";
    }

    class MagellanBot : IBot
    {
        private MagellanBotConfig cfg = new MagellanBotConfig();
        public IBotConfig Config => cfg;

        public void Init(CommandLineParser cmd)
        {
            
        }

        public Task ClientReady() => Task.CompletedTask;

        public async Task ProcessMessage(string[] msg, SocketUserMessage userMsg)
        {
            foreach (var m in msg)
            {
                if (Globals.Commands.ContainsKey(m))
                    await Task.Run(() => Globals.Commands[m].Invoke(userMsg));
            }
        }
    }
}
