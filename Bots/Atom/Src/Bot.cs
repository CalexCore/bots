using System.Threading.Tasks;
using AngryWasp.Helpers;
using Nerva.Bots.Plugin;

namespace Atom
{
    public class AtomBotConfig : IBotConfig
    {
        public ulong OwnerID => 388852986316587010;
        public ulong ServerID => 479322729007546368;
        //FORK: Set the correct bot id and remove this comment
        public ulong BotID => 0;
        public ulong BotChannelID => 509444814404714501;
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
