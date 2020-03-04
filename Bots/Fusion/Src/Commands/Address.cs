using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;

namespace Fusion.Commands
{
    [Command("address", "Display your fusion tipjar address")]
    public class Address : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                AccountHelper.CreateNewAccount(msg);
            else
                Sender.PrivateReply(msg, $"`{cfg.UserWalletCache[msg.Author.Id].Item2}`");
        }
    }
}