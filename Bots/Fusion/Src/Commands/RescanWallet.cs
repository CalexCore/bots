using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;
using Nerva.Rpc.Wallet;

namespace Fusion.Commands
{
    [Command("rescan", "Rescans the wallet to fix any funkiness. Not for you")]
    public class RescanWallet : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            //can only be run by angrywasp
            //todo: remove hard coded user id
            if (msg.Author.Id != 407511685134549003)
            {
                await Sender.PublicReply(msg, "No rescan for you!");
                return;
            }

            await new RescanBlockchain( (c) =>
            {
                Sender.PrivateReply(msg, "Rescan of donation wallet complete.").Wait();
            }, (e) =>
            {
                Sender.PrivateReply(msg, "Oof. Couldn't rescan donation wallet.").Wait();
            }, cfg.WalletHost, cfg.DonationWalletPort).RunAsync();

            await new RescanBlockchain( (c) =>
            {
                Sender.PrivateReply(msg, "Rescan of user wallet complete.").Wait();
            }, (e) =>
            {
                Sender.PrivateReply(msg, "Oof. Couldn't rescan user wallet.").Wait();
            }, cfg.WalletHost, cfg.UserWalletPort).RunAsync();
        }
    }
}