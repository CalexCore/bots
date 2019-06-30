using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;

namespace Fusion.Commands
{
    [Command("address", "Display your fusion tipjar address")]
    public class Address : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletIndices.ContainsKey(msg.Author.Id))
                AccountHelper.CreateNewAccount(msg);
            else
            {
                uint accountIndex = cfg.UserWalletIndices[msg.Author.Id];

                new GetAddress(new GetAddressRequestData
                {
                    AccountIndex = accountIndex
                }, (GetAddressResponseData r) =>
                {
                    Sender.PrivateReply(msg, $"`{r.Address}`").Wait();
                }, (RequestError e) =>
                {
                    Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
                },
                cfg.WalletHost, cfg.UserWalletPort).Run();
            }
        }
    }
}