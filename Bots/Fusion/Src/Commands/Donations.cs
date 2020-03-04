using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Nerva.Rpc.Wallet;

namespace Fusion.Commands
{
    [Command("donations", "Get information on previous donations")]
    public class Donations : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);
            GetAccountsResponseData balances = null;
            List<GetTransfersResponseData> transfers = new List<GetTransfersResponseData>();

            new GetAccounts(null, (GetAccountsResponseData result) =>
            {
                balances = result;
            }, null, cfg.WalletHost, cfg.DonationWalletPort).Run();

            if (balances == null)
            {
                DiscordResponse.Reply(msg, text: "Sorry. Couldn't retrieve the donation account balances...");
                return;
            }

            foreach (var a in cfg.AccountJson.Accounts)
            {
                if (!a.Display)
                {
                    transfers.Add(null);
                    continue;
                }

                new GetTransfers(new GetTransfersRequestData
                {
                    AccountIndex = (uint)a.Index
                }, (GetTransfersResponseData result) =>
                {
                    transfers.Add(result);
                }, null, cfg.WalletHost, cfg.DonationWalletPort).Run();
            }

            if (cfg.AccountJson.Accounts.Length != transfers.Count)
            {
                DiscordResponse.Reply(msg, text: "Sorry. Couldn't retrieve the donation account balances...");
                return;
            }

            EmbedBuilder eb = new EmbedBuilder()
            .WithAuthor("Donation Account Balances", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("These are the balances of all the donation accounts")
            .WithColor(Color.Red)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            for (int i = 0; i < cfg.AccountJson.Accounts.Length; i++)
            {
                var a = cfg.AccountJson[i];

                if (!a.Display)
                    continue;

                var bb = balances.Accounts[i].UnlockedBalance;
                var tt = transfers[i];

                StringBuilder sb = new StringBuilder();

                foreach (var t in tt.Incoming.OrderByDescending(x => x.Height).Take(5))
                {
                    ulong s, r;
                    IdEncrypter.Decrypt(t.PaymentId, out s, out r);
                    sb.AppendLine($"{s.ToMention()}: {t.Amount.FromAtomicUnits()}");
                }

                sb.AppendLine("\u200b");

                ulong outTotal = 0;
                foreach (var o in tt.Outgoing)
                    outTotal += o.Amount;

                eb.AddField($"{a.Name}: {bb.FromAtomicUnits()} xam ({outTotal.FromAtomicUnits()} out)", sb.ToString());
            }

            DiscordResponse.Reply(msg, embed: eb.Build());
        }
    }
}