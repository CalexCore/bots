using System;
using System.IO;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using Newtonsoft.Json;
using Log = Nerva.Bots.Helpers.Log;
using System.Collections.Generic;
using Fusion.Commands.Gaming;

namespace Fusion
{
    public class FusionBotConfig : IBotConfig
    {
		//todo: set correct bot id when created. delete this todo
		public ulong BotId => 466512207396732939;

        public List<ulong> BotChannelIds => new List<ulong>
		{
			509444814404714501, // Amity bots channel

		};

		public List<ulong> DevRoleIds => new List<ulong>
		{
			556604722476351490 //Amity admin role
		};

        public string CmdPrefix => "$";

		public string WalletHost { get; } = "http://127.0.0.1";

        public uint DonationWalletPort { get; set; } = (uint)MathHelper.Random.NextInt(10000, 50000);

		public uint UserWalletPort { get; set; } = (uint)MathHelper.Random.NextInt(10000, 50000);

        public AccountJson AccountJson { get; set; } = null;

        public string DonationPaymentIdKey { get; set; } = null;

		public Dictionary<ulong, Tuple<uint, string>> UserWalletCache { get; } = new Dictionary<ulong, Tuple<uint, string>>();
    }

    public class FusionBot : IBot
    {
        private FusionBotConfig cfg = new FusionBotConfig();
        public IBotConfig Config => cfg;

        public Task ClientReady() => Task.CompletedTask;

        public void Init(CommandLineParser cmd)
        {
			AngryWasp.Serializer.Serializer.Initialize();

            if (cmd["donation-wallet-port"] != null)
				cfg.DonationWalletPort = uint.Parse(cmd["donation-wallet-port"].Value);

			if (cmd["user-wallet-port"] != null)
				cfg.UserWalletPort = uint.Parse(cmd["user-wallet-port"].Value);

			string donationWalletFile = string.Empty, donationWalletPassword = string.Empty;
			string userWalletFile = string.Empty, userWalletPassword = string.Empty;

			if (cmd["key-file"] != null)
			{
				string[] keys = File.ReadAllLines(cmd["key-file"].Value);
				string keyFilePassword = PasswordPrompt.Get("Please enter the key file decryption password");

				donationWalletPassword = keys[0].Decrypt(keyFilePassword);
				userWalletPassword = keys[1].Decrypt(keyFilePassword);
				cfg.DonationPaymentIdKey = keys[2].Decrypt(keyFilePassword);

				keyFilePassword = null;
			}
			else
			{
				donationWalletPassword = PasswordPrompt.Get("Please enter the donation wallet password");
				userWalletPassword = PasswordPrompt.Get("Please enter the user wallet password");
				cfg.DonationPaymentIdKey = PasswordPrompt.Get("Please enter the payment id encryption key");
			}

			if (cmd["donation-wallet-file"] != null)
				donationWalletFile = cmd["donation-wallet-file"].Value;

			if (cmd["user-wallet-file"] != null)
				userWalletFile = cmd["user-wallet-file"].Value;

			string jsonFile = Path.Combine(Environment.CurrentDirectory, $"Wallets/{donationWalletFile}.json");
			Log.Write($"Loading Wallet JSON: {jsonFile}");
			cfg.AccountJson = JsonConvert.DeserializeObject<AccountJson>(File.ReadAllText(jsonFile));

			new OpenWallet(new OpenWalletRequestData 
			{
				FileName = donationWalletFile,
				Password = donationWalletPassword
			},
			(string result) => 
			{
				Log.Write("Wallet loaded");
			},
			(RequestError error) => 
			{
				Log.Write("Failed to load donation wallet");
				Environment.Exit(1);
			}, 
			cfg.WalletHost, cfg.DonationWalletPort).Run();

			new OpenWallet(new OpenWalletRequestData {
				FileName = userWalletFile,
				Password = userWalletPassword
			},
			(string r1) =>
			{
				Log.Write("Wallet loaded");
				new GetAccounts(null, (GetAccountsResponseData r2) =>
				{
					foreach (var a in r2.Accounts)
					{
						ulong uid = 0;

						if (ulong.TryParse(a.Label, out uid))
						{
							if (!cfg.UserWalletCache.ContainsKey(uid))
							{
								cfg.UserWalletCache.Add(uid, new Tuple<uint, string>(a.Index, a.BaseAddress));
								Log.Write($"Loaded wallet for user: {a.Label} - {a.BaseAddress}");
							}
							else
								Log.Write($"Duplicate wallet detected for user with id: {uid}");
						}
						else
						{
							//fusion owns address index 0
							if (a.Index == 0)
								cfg.UserWalletCache.Add(cfg.BotId, new Tuple<uint, string>(a.Index, a.BaseAddress));
							else
								Log.Write($"Account index {a.Index} is not associated with a user");
						}
					}
				},
				(RequestError error) =>
				{
					Log.Write("Failed to load user wallet");
					Environment.Exit(1);
				},
				cfg.WalletHost, cfg.UserWalletPort).Run();
			},
			(RequestError error) =>
			{
				Log.Write("Failed to load user wallet");
				Environment.Exit(1);
			},
			cfg.WalletHost, cfg.UserWalletPort).Run();

			//todo: Check if an existing game is still running after restart and load that instead

			string fp = Path.Combine(Environment.CurrentDirectory, "lottery.xml");

			if (File.Exists(fp))
				LotteryManager.Load(fp);
			else
				LotteryManager.Start();
        }
    }
}