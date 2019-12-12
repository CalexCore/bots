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
    [Command("balance", "Get information on your balance")]
    public class Balance : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                await AccountHelper.CreateNewAccount(msg);
            else
            {
                await new GetBalance(new GetBalanceRequestData
                {
                    AccountIndex = cfg.UserWalletCache[msg.Author.Id].Item1
                },
                (GetBalanceResponseData result) =>
                {
                    EmbedBuilder eb = new EmbedBuilder()
                    .WithAuthor($"Your Tip Jar", Globals.Client.GetUser(msg.Author.Id).GetAvatarUrl())
                    .WithDescription("Let's see what's going on here")
                    .WithColor(Color.DarkTeal)
                    .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

                    eb.AddField("Address", cfg.UserWalletCache[msg.Author.Id].Item2);
                    eb.AddField($"Unlocked", $"{result.UnlockedBalance.FromAtomicUnits()} xam");
                    eb.AddField($"Total", $"{result.Balance.FromAtomicUnits()} xam");

                    Sender.PrivateReply(msg, null, eb.Build()).Wait();
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