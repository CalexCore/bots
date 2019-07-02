using System.Collections.Generic;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Nerva.Bots.Plugin;

namespace Atom
{
    public class AtomBotConfig : IBotConfig
    {
        public ulong OwnerId => 388852986316587010;

        //FORK: Set the correct bot id and remove this comment
        public ulong BotId => 0;

        public List<ulong> BotChannelIds => new List<ulong>
		{
			509444814404714501, //AM-Bots
		};

		public List<ulong> DevRoleIds => new List<ulong>
		{
			556604722476351490, //Admin
		};

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
