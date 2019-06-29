using System.Threading.Tasks;
using AngryWasp.Helpers;
using Nerva.Bots.Plugin;

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
