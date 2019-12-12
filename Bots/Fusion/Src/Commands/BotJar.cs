using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using Nerva.Bots.Helpers;
using System.Threading.Tasks;

namespace Fusion.Commands
{
    [Command("botjar", "Get information about fusion's wallet")]
    public class BotJar : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            await new GetBalance(new GetBalanceRequestData
            {
                AccountIndex = 0
            },
            (GetBalanceResponseData result) =>
            {
                EmbedBuilder eb = new EmbedBuilder()
                .WithAuthor($"Billys's Tip Jar", Globals.Client.CurrentUser.GetAvatarUrl())
                .WithDescription("Whale or fail?")
                .WithColor(Color.DarkTeal)
                .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

                eb.AddField("Address", cfg.UserWalletCache[cfg.BotId].Item2);
                eb.AddField("Unlocked", $"{result.UnlockedBalance.FromAtomicUnits()} xam");
                eb.AddField("Total", $"{result.Balance.FromAtomicUnits()} xam");

                Sender.PublicReply(msg, null, eb.Build()).Wait();
            },
            (RequestError e) =>
            {
                Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
            },
            cfg.WalletHost, cfg.UserWalletPort).RunAsync();
        }
    }
}