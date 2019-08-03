using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;

namespace Fusion.Commands.Gaming
{
    [Command("buytickets", "Buy some lottery tickets")]
    public class BuyTickets : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                await AccountHelper.CreateNewAccount(msg);
            else
            {
                int amount = 0;
                if (!AccountHelper.ParseIntFromMessage(msg, out amount) || amount == 0)
                {
                    await Sender.PublicReply(msg, "Oof. No good. You didn't say how many tickets you want.");
                    return;
                }

                double playerBalance = 0;

                //get balance of player wallet
                await new GetBalance(new GetBalanceRequestData {
                    AccountIndex = cfg.UserWalletCache[msg.Author.Id].Item1
                }, (GetBalanceResponseData result) => {
                    playerBalance = result.UnlockedBalance.FromAtomicUnits() - 0.1;
                }, (RequestError e) => {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
                }, cfg.WalletHost, cfg.UserWalletPort).RunAsync();

                //if you can't afford all you have asked for, then you only get what you can afford
                amount = Math.Min((int)(playerBalance / LotteryManager.CurrentGame.Parameters.TicketCost), amount);

                int[] allocatedTickets =await  LotteryManager.CurrentGame.AllocateTickets(msg, amount);
                if (allocatedTickets.Length == 0)
                    return;
            }
        }
    }
}