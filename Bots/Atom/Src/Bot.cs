using System.Collections.Generic;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Nerva.Bots.Plugin;

namespace Atom
{
    public class AtomBotConfig : IBotConfig
    {
        public ulong BotId => 450609948246671360;

        public List<ulong> BotChannelIds => new List<ulong>
		{
			450660331405049876, //Atom
			595232529456562198, //CB-General
			595231506209701908, //CB-ST
            504717279573835832, //AM-XNV
			509444814404714501, //AM-Bots
            510621605479710720, //LB-General
		};

        public List<ulong> DevRoleIds => new List<ulong>
		{
			595498219987927050, //NV-BotCommander
            595495919097741322, //AM-BotCommander
            595495392632766474, //LB-BotCommander
		};
        
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

        public IBotConfig Config => cfg;

        public void Init(CommandLineParser cmd)
        {
        }

        public Task ClientReady()
        {
            return Task.CompletedTask;
        }
    }
}
