using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;

namespace Fusion.Commands.Gaming
{
    [Command("buytickets", "Buy some lottery tickets")]
    public class BuyTickets : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                AccountHelper.CreateNewAccount(msg);
            else
            {
            }
        }
    }
}