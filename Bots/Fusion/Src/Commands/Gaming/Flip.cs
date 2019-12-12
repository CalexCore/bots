using System.Collections.Generic;
using AngryWasp.Helpers;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using System.Threading.Tasks;

namespace Fusion.Commands.Gaming
{
    [Command("flip", "Toss a coin. Double or nothing")]
    public class Flip : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                await AccountHelper.CreateNewAccount(msg);
            else
            {
                double betAmount;
                if (!AccountHelper.ParseDoubleFromMessage(msg, out betAmount))
                {
                    await Sender.PublicReply(msg, "Oof. No good. You didn't say how much you want to bet.");
                    return;
                }

                ulong totalAmount = betAmount.ToAtomicUnits() + (0.1d).ToAtomicUnits();

                //both parties must have the amount + 0.1xam to cover potential fees

                uint playerAccountIndex = cfg.UserWalletCache[msg.Author.Id].Item1;
                string playerAddress = cfg.UserWalletCache[msg.Author.Id].Item2;
                ulong playerBalance = 0;

                string fusionAddress = cfg.UserWalletCache[cfg.BotId].Item2;
                ulong fusionBalance = 0;

                //get balance of player wallet
                await new GetBalance(new GetBalanceRequestData {
                    AccountIndex = cfg.UserWalletCache[msg.Author.Id].Item1
                }, (GetBalanceResponseData result) => {
                    playerBalance = result.UnlockedBalance;
                }, (RequestError e) => {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
                }, cfg.WalletHost, cfg.UserWalletPort).RunAsync();

                //get balance of fusion wallet
                await new GetBalance(new GetBalanceRequestData {
                    AccountIndex = cfg.UserWalletCache[cfg.BotId].Item1
                }, (GetBalanceResponseData result) => {
                    fusionBalance = result.UnlockedBalance;
                }, (RequestError e) => {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
                }, cfg.WalletHost, cfg.UserWalletPort).RunAsync();

                if (playerBalance < totalAmount)
                {
                    await Sender.PublicReply(msg, "You ain't got enough cash. Maybe you need gamblers anonymous? :thinking:");
                    return;
                }

                if (fusionBalance < totalAmount)
                {
                    await Sender.PublicReply(msg, "Hold on high roller! I can't cover that :whale:");
                    return;
                }

                double d = MathHelper.Random.NextDouble();

                if (d > 0.5d) //payout
                {
                    RequestError err = await AccountHelper.PayUser(betAmount, cfg.BotId, msg.Author.Id);
                    await HandlePayoutResult(msg, true, err);
                }
                else //take it
                {
                    RequestError err = await AccountHelper.PayUser(betAmount, msg.Author.Id, cfg.BotId);
                    await HandlePayoutResult(msg, false, err);
                }
            }
        }

        private async Task HandlePayoutResult(SocketUserMessage msg, bool win, RequestError err)
        {
            if (err != null)
                await Sender.PrivateReply(msg, $"{msg.Author.Mention} Oops. RPC Error: {err.Code}: {err.Message}", null); 
            else
            {
                if (win)
                    await Sender.PublicReply(msg, $"{msg.Author.Mention} Winner winner, chicken dinner! :chicken:");
                else
                    await Sender.PublicReply(msg, $"{msg.Author.Mention} You lose, sucker! :joy:");
            }
        }
    }
}