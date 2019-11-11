using System.Threading.Tasks;
using AngryWasp.Helpers;

namespace Nerva.Bots.Plugin
{
    public interface IBot
    {
        void Init(CommandLineParser cmd);

        Task ClientReady();

        IBotConfig Config { get; }
    }
}