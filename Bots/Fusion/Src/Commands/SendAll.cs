using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;

namespace Fusion.Commands
{
    [Command("sendall", "Empty your bag")]
    public class SendAll : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                AccountHelper.CreateNewAccount(msg);
            else
            {
                string address;
                if (!AccountHelper.ParseAddressFromMessage(msg, out address))
                {
                    Sender.PrivateReply(msg, "Oof. No good. You didn't provide a valid address. :derp:").Wait();
                    return;
                }

                uint accountIndex = cfg.UserWalletCache[msg.Author.Id].Item1;

                new SweepAll(new SweepAllRequestData
                {
                    AccountIndex = accountIndex,
                    Address = address,
                },
                (SweepAllResponseData r) =>
                {
                    int numTxs = r.TxHashList.Count;
                    double totalAmount = 0, totalFee = 0;
                    string txHashList = string.Empty;

                    for (int i = 0; i < numTxs; i++)
                    {
                        totalAmount += r.AmountList[i].FromAtomicUnits();
                        totalFee += r.FeeList[i].FromAtomicUnits();
                        txHashList += $"{r.TxHashList[i]}\r\n";
                    }

                    Sender.PrivateReply(msg, $"{totalAmount} xnv was sent with a fee of {totalFee} xnv in {numTxs} transactions\r\n{txHashList}").Wait();
                },
                (RequestError e) =>
                {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
                },
                cfg.WalletHost, cfg.UserWalletPort).Run();
            }
        }
    }
}