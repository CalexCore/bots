using System.Threading.Tasks;
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
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                await AccountHelper.CreateNewAccount(msg);
            else
            {
                string address;
                if (!AccountHelper.ParseAddressFromMessage(msg, out address))
                {
                    await Sender.PrivateReply(msg, "Oof. No good. You didn't provide a valid address. :derp:");
                    return;
                }

                uint accountIndex = cfg.UserWalletCache[msg.Author.Id].Item1;

                await new SweepAll(new SweepAllRequestData
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

                    Sender.PrivateReply(msg, $"{totalAmount} xam was sent with a fee of {totalFee} xam in {numTxs} transactions\r\n{txHashList}").Wait();
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