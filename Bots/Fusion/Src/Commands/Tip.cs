using System.Collections.Generic;
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
        public void Process(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            if (!cfg.UserWalletCache.ContainsKey(msg.Author.Id))
                AccountHelper.CreateNewAccount(msg);
            else
            {
                double amount;
                if (!AccountHelper.ParseDoubleFromMessage(msg, out amount))
                {
                    Sender.PrivateReply(msg, "Oof. No good. You didn't say how much you want to tip.");
                    return;
                }

                uint accountIndex = cfg.UserWalletCache[msg.Author.Id].Item1;

                foreach (var m in msg.MentionedUsers)
                {
                    if (cfg.UserWalletCache.ContainsKey(m.Id))
                        SendToUser(msg, m, accountIndex, amount.ToAtomicUnits());
                    else
                    {
                        string address = AccountHelper.CreateNewAccount(m);
                        if (string.IsNullOrEmpty(address))
                        {
                            Sender.PrivateReply(msg, $"{m.Mention} does not have a wallet. They cannot take your tip.");
                            Sender.SendPrivateMessage(Globals.Client.GetUser(m.Id), $"{msg.Author.Mention} tried to send you {amount} xmr, but you don't have a wallet");
                        }
                        else
                            SendToUser(msg, m, accountIndex, amount.ToAtomicUnits());
                    }
                }
            }
        }

        private void SendToUser(SocketUserMessage msg, SocketUser recipient, uint accountIndex, ulong amount)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);

            new Transfer(new TransferRequestData
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
                Sender.SendPrivateMessage(Globals.Client.GetUser(msg.Author.Id), $"You sent {r.Amount.FromAtomicUnits()} xmr to {recipient.Mention} with a fee of {r.Fee.FromAtomicUnits()} xmr\r\n{r.TxHash}");
                msg.AddReactionAsync(new Emoji("💸"));

                if (recipient.Id != cfg.BotId) //exception thrown if trying to send a DM to fusion, so skip
                    Sender.SendPrivateMessage(Globals.Client.GetUser(recipient.Id), $"{msg.Author.Username} sent you {r.Amount.FromAtomicUnits()} xmr");
            },
            (RequestError e) =>
            {
                Sender.SendPrivateMessage(Globals.Client.GetUser(msg.Author.Id), "Oof. No good. You are going to have to try again later.");
                msg.AddReactionAsync(new Emoji("🆘"));
            },
            cfg.WalletHost, cfg.UserWalletPort).Run();
        }
    }
}