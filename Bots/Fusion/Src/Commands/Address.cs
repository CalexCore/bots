using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;

namespace Fusion.Commands
{
    [Command("address", "Display your fusion tipjar address")]
    public class Address : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                await AccountHelper.CreateNewAccount(msg);
            else
            {
                string address = cfg.UserWalletCache[msg.Author.Id].Item2;
                await Sender.PrivateReply(msg, $"`{address}`");
            }
        }
    }
}