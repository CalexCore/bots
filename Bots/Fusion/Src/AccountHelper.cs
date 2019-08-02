using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using System.Linq;
using System;

namespace Fusion
{
    public static class AccountHelper
    {
        public static void CreateNewAccount(SocketUserMessage msg)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);
            ulong id = msg.Author.Id;
            
            //todo: should probably attempt to fetch a wallet with a tag matching the user id before making one
            Sender.PrivateReply(msg, "You don't have an account. Hold while I create one for you.").Wait();

            //create an account
            new CreateAccount(new CreateAccountRequestData
            {
                Label = id.ToString()
            },
            (CreateAccountResponseData r) =>
            {
                Sender.PrivateReply(msg, $"You now have a new account. You can make a deposit to\r\n`{r.Address}`").Wait();
                cfg.UserWalletCache.Add(id, new Tuple<uint, string>(r.Index, r.Address));
            },
            (RequestError e) =>
            {
                Sender.PrivateReply(msg, "Oof. No good. You are going to have to try again later.").Wait();
            },
            cfg.WalletHost, cfg.UserWalletPort).Run();
        }

        public static bool CreateNewAccount(SocketUser user, out string address)
        {
            FusionBotConfig cfg = ((FusionBotConfig)Globals.Bot.Config);
            bool ret = false;
            string addr = string.Empty;
            
            if (cfg.UserWalletCache.ContainsKey(user.Id))
            {
                address = cfg.UserWalletCache[user.Id] == null ? string.Empty : cfg.UserWalletCache[user.Id].Item2;
                return !string.IsNullOrEmpty(address);
            }

            cfg.UserWalletCache.Add(user.Id, null);

            new CreateAccount(new CreateAccountRequestData
            {
                Label = user.Id.ToString()
            },
            (CreateAccountResponseData r) =>
            {
                Sender.SendPrivateMessage(user, $"You now have a new account. You can make a deposit to\r\n`{r.Address}`").Wait();
                cfg.UserWalletCache[user.Id] = new Tuple<uint, string>(r.Index, r.Address);
                addr = r.Address;
                ret = true;
            },
            null, cfg.WalletHost, cfg.UserWalletPort).Run();

            address = addr;
            return ret;
        }

        public static bool ParseAddressFromMessage(SocketUserMessage msg, out string address)
        {
            address = null;
            List<string> addresses = new Regex(@"(NV|NS)[a-zA-Z0-9]{95}").Matches(msg.Content).Select(x => x.Value).ToList();
            addresses.AddRange(new Regex(@"(Ni)[a-zA-Z0-9]{107}").Matches(msg.Content).Select(x => x.Value).ToList());

            if (addresses.Count == 0)
                return false;

            //todo: need to properly validate the address
            address = addresses[0];
            return true;
        }

        public static bool ParseDoubleFromMessage(SocketUserMessage msg, out double amount)
        {
            Regex amtPattern = new Regex(@"\s(\d+(\.?\d+)*)");
            var m = amtPattern.Match(msg.Content);

            if (m.Success)
                double.TryParse(m.Value, out amount);
            else
                amount = double.NaN;

            return !double.IsNaN(amount);
        }

        public static bool ParseIntFromMessage(SocketUserMessage msg, out int amount)
        {
            
            Regex amtPattern = new Regex(@"\s(\d+(\.?\d+)*)");
            var m = amtPattern.Match(msg.Content);

            if (m.Success)
                int.TryParse(m.Value, out amount);
            else
                amount = int.MinValue;

            return amount != int.MaxValue;;
        }
    }
}