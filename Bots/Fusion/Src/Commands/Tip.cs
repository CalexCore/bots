using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;

namespace Fusion.Commands
{
    [Command("tip", "Send someone some coins")]
    public class Tip : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                await AccountHelper.CreateNewAccount(msg);
            else
            {
                double amount;
                if (!AccountHelper.ParseDoubleFromMessage(msg, out amount))
                {
                    await Sender.PrivateReply(msg, "Oof. No good. You didn't say how much you want to tip.");
                    return;
                }

                uint accountIndex = cfg.UserWalletCache[msg.Author.Id].Item1;

                foreach (var m in msg.MentionedUsers)
                {
                    if (cfg.UserWalletCache.ContainsKey(m.Id))
                        await SendToUser(msg, m, accountIndex, amount.ToAtomicUnits());
                    else
                    {
                        string address = await AccountHelper.CreateNewAccount(m);
                        if (!string.IsNullOrEmpty(address))
                        {
                            await Sender.PrivateReply(msg, $"{m.Mention} does not have a wallet. They cannot take your tip.");
                            await Sender.SendPrivateMessage(Globals.Client.GetUser(m.Id), $"{msg.Author.Mention} tried to send you {amount} xam, but you don't have a wallet");
                        }
                        else
                            await SendToUser(msg, m, accountIndex, amount.ToAtomicUnits());
                    }
                }
            }
        }

        private async Task SendToUser(SocketUserMessage msg, SocketUser recipient, uint accountIndex, ulong amount)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            await new Transfer(new TransferRequestData
            {
                AccountIndex = accountIndex,
                Destinations = new List<TransferDestination> {
                    new TransferDestination {
                        Amount = amount,
                        Address = cfg.UserWalletCache[recipient.Id].Item2
                }}
            },
            (TransferResponseData r) =>
            {
                Sender.SendPrivateMessage(Globals.Client.GetUser(msg.Author.Id), $"You sent {r.Amount.FromAtomicUnits()} xam to {recipient.Mention} with a fee of {r.Fee.FromAtomicUnits()} xam\r\n{r.TxHash}").Wait();
                msg.AddReactionAsync(new Emoji("💸"));

                if (recipient.Id != cfg.BotId) //exception thrown if trying to send a DM to fusion, so skip
                    Sender.SendPrivateMessage(Globals.Client.GetUser(recipient.Id), $"{msg.Author.Mention} sent you {r.Amount.FromAtomicUnits()} xam").Wait();
            },
            (RequestError e) =>
            {
                Sender.SendPrivateMessage(Globals.Client.GetUser(msg.Author.Id), "Oof. No good. You are going to have to try again later.").Wait();
                msg.AddReactionAsync(new Emoji("🆘"));
            },
            cfg.WalletHost, cfg.UserWalletPort).RunAsync();
        }
    }
}