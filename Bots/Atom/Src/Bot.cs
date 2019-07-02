using System.Collections.Generic;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Nerva.Bots.Plugin;

namespace Atom
{
    public class AtomBotConfig : IBotConfig
    {
        public ulong OwnerId => 407511685134549003;

        public ulong BotId => 450609948246671360;

        public List<ulong> BotChannelIds => new List<ulong>
		{
			450660331405049876, //Atom
			595232529456562198, //General
			595231506209701908, //S-T
		};

        public List<ulong> DevRoleIds => new List<ulong>
		{
			487081227459887146,
			450818384901308442,
			450818386759254028
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
