using System.Collections.Generic;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using Nerva.Bots.Helpers;
using System.Threading.Tasks;

namespace Fusion.Commands
{
    [Command("send", "Send some coins to one or more addresses")]
    public class Send : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                await AccountHelper.CreateNewAccount(msg);
            else
            {
                uint accountIndex = cfg.UserWalletCache[msg.Author.Id].Item1;

                string address;
                double amount;

                if (!AccountHelper.ParseAddressFromMessage(msg, out address))
                {
                    await Sender.PrivateReply(msg, "Oof. No good. You didn't provide a valid address. :derp:");
                    return;
                }

                if (!AccountHelper.ParseDoubleFromMessage(msg, out amount))
                {
                    await Sender.PrivateReply(msg, "Oof. No good. You need to know how much you want to send. :derp:");
                    return;
                }

                await new Transfer(new TransferRequestData
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
                    Sender.PrivateReply(msg, $"{r.Amount.FromAtomicUnits()} xnv was sent with a fee of {r.Fee.FromAtomicUnits()} xnv\r\n{r.TxHash}").Wait();
                },
                (RequestError e) =>
                {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
                },
                cfg.WalletHost, cfg.UserWalletPort).RunAsync();
            }
        }
    }
}