using System.Collections.Generic;
using AngryWasp.Helpers;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using static AngryWasp.Helpers.MathHelper;

namespace Fusion.Commands
{
    [Command("flip", "Toss a coin. Double or nothing")]
    public class Flip : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                AccountHelper.CreateNewAccount(msg);
            else
            {
                double betAmount;
                if (!AccountHelper.ParseAmountFromMessage(msg, out betAmount))
                {
                    Sender.PublicReply(msg, "Oof. No good. You didn't say how much you want to bet.").Wait();
                    return;
                }

                ulong totalAmount = betAmount.ToAtomicUnits() + (0.1d).ToAtomicUnits();

                //both parties must have the amount + 0.1xnv to cover potential fees

                uint playerAccountIndex = cfg.UserWalletCache[msg.Author.Id].Item1;
                string playerAddress = cfg.UserWalletCache[msg.Author.Id].Item2;
                ulong playerBalance = 0;

                string fusionAddress = cfg.UserWalletCache[cfg.BotID].Item2;
                ulong fusionBalance = 0;

                //get balance of player wallet
                new GetBalance(new GetBalanceRequestData {
                    AccountIndex = cfg.UserWalletCache[msg.Author.Id].Item1
                }, (GetBalanceResponseData result) => {
                    playerBalance = result.UnlockedBalance;
                }, (RequestError e) => {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
                }, cfg.WalletHost, cfg.UserWalletPort).Run();

                //get balance of fusion wallet
                new GetBalance(new GetBalanceRequestData {
                    AccountIndex = cfg.UserWalletCache[cfg.BotID].Item1
                }, (GetBalanceResponseData result) => {
                    fusionBalance = result.UnlockedBalance;
                }, (RequestError e) => {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
                }, cfg.WalletHost, cfg.UserWalletPort).Run();

                if (playerBalance < totalAmount)
                {
                    Sender.PublicReply(msg, "You ain't got enough cash. Maybe you need gamblers anonymous? :thinking:").Wait();
                    return;
                }

                if (fusionBalance < totalAmount)
                {
                    Sender.PublicReply(msg, "Hold on high roller! I can't cover that :whale:").Wait();
                    return;
                }

                double d = new MersenneTwister(Random.GenerateRandomSeed()).NextDouble();

                if (d > 0.5d) //payout
                {
                    RequestError err;
                    Payout(betAmount, cfg.BotID, msg.Author.Id, out err);
                    HandlePayoutResult(msg, true, err);
                }
                else //take it
                {
                    RequestError err;
                    Payout(betAmount, msg.Author.Id, cfg.BotID, out err);
                    HandlePayoutResult(msg, false, err);
                }
            }
        }

        public static bool Payout(double amount, ulong sender, ulong recipient, out RequestError err)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            RequestError error = null;
            bool ret = true;

            new Transfer(new TransferRequestData
            {
                AccountIndex = cfg.UserWalletCache[sender].Item1,
                Destinations = new List<TransferDestination> {
                    new TransferDestination {
                        Amount = amount.ToAtomicUnits(),
                        Address = cfg.UserWalletCache[recipient].Item2
                    }}
            }, null, (RequestError e) =>
            {
                error = e;
                ret = false;
            },
            cfg.WalletHost, cfg.UserWalletPort).Run();
            
            err = error;
            return ret;
        }

        public static void HandlePayoutResult(SocketUserMessage msg, bool win, RequestError err)
        {
            if (err != null)
                Sender.PrivateReply(msg, $"{msg.Author.Mention} Oops. RPC Error: {err.Code}: {err.Message}", null).Wait(); 
            else
            {
                if (win)
                    Sender.PublicReply(msg, $"{msg.Author.Mention} Winner winner, chicken dinner! :chicken:").Wait();
                else
                    Sender.PublicReply(msg, $"{msg.Author.Mention} You lose, sucker! :joy:").Wait();
            }
        }
    }
}