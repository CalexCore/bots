using System.Collections.Generic;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Nerva.Bots.Plugin;

namespace Atom
{
    public class AtomBotConfig : IBotConfig
    {
        //todo: set correct bot id when created. delete this todo
        public ulong BotId => 450609948246671360;

        public List<ulong> BotChannelIds => new List<ulong>
		{
			509444814404714501, // Amity bots channel
		};

        public List<ulong> DevRoleIds => new List<ulong>
		{
			556604722476351490 //Amity admin role
		};
        
        public string CmdPrefix => "!";

        //todo: set seed node addresses and potentially change paths
        //bot expects <seed_node_url>/api/...
        public static readonly string[] SeedNodes = new string[]
        {
            "http://s3.xam.xyz",
            "http://s4.xam.xyz"
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
