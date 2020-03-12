using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using Nerva.Bots.Helpers;

namespace Fusion.Commands
{
    [Command("botjar", "Get information about the bot wallet")]
    public class BotJar : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            new GetBalance(new GetBalanceRequestData
            {
                AccountIndex = 0
            },
            (GetBalanceResponseData result) =>
            {
                EmbedBuilder eb = new EmbedBuilder()
                .WithAuthor($"Tip Jar", Globals.Client.CurrentUser.GetAvatarUrl())
                .WithDescription("Whale or fail?")
                .WithColor(Color.DarkTeal)
                .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

                eb.AddField("Address", cfg.UserWalletCache[cfg.BotId].Item2);
                eb.AddField("Unlocked", $"{result.UnlockedBalance.FromAtomicUnits()} xmr");
                eb.AddField("Total", $"{result.Balance.FromAtomicUnits()} xmr");

                Sender.PublicReply(msg, null, eb.Build());
            },
            (RequestError e) =>
            {
                Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.");
            },
            cfg.WalletHost, cfg.UserWalletPort).Run();
        }
    }
}
