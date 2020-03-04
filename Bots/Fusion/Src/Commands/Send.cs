using System.Collections.Generic;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using Nerva.Bots.Helpers;

namespace Fusion.Commands
{
    [Command("send", "Send some coins to one or more addresses")]
    public class Send : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                AccountHelper.CreateNewAccount(msg);
            else
            {
                uint accountIndex = cfg.UserWalletCache[msg.Author.Id].Item1;

                string address;
                double amount;

                if (!AccountHelper.ParseAddressFromMessage(msg, out address))
                {
                    Sender.PrivateReply(msg, "Oof. No good. You didn't provide a valid address. :derp:");
                    return;
                }

                if (!AccountHelper.ParseDoubleFromMessage(msg, out amount))
                {
                    Sender.PrivateReply(msg, "Oof. No good. You need to know how much you want to send. :derp:");
                    return;
                }

                new Transfer(new TransferRequestData
                {
                    AccountIndex = accountIndex,
                    Destinations = new List<TransferDestination> {
                        new TransferDestination {
                            Amount = amount.ToAtomicUnits(),
                            Address = address
                        }}
                },
                (TransferResponseData r) =>
                {
                    Sender.PrivateReply(msg, $"{r.Amount.FromAtomicUnits()} xam was sent with a fee of {r.Fee.FromAtomicUnits()} xam\r\n{r.TxHash}");
                },
                (RequestError e) =>
                {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.");
                },
                cfg.WalletHost, cfg.UserWalletPort).Run();
            }
        }
    }
}