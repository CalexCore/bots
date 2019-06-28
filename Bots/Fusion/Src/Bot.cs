using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using Newtonsoft.Json;
using Log = Nerva.Bots.Helpers.Log;

namespace Fusion
{
     public class FusionBotConfig : IBotConfig
    {
        public ulong OwnerID => 407511685134549003;
        public ulong ServerID => 439649936414474256;
        public ulong BotID => 466512207396732939;
        public ulong BotChannelID => 466873635638870016;
        public string CmdPrefix => "$";

		public string WalletHost { get; } = "http://127.0.0.1";

        public uint WalletPort { get; set; } = (uint)MathHelper.Random.NextInt(10000, 50000);

        public AccountJson AccountJson { get; set; } = null;

        public string DonationPaymentIdKey { get; set; } = null;
    }

    public class FusionBot : IBot
    {
        private FusionBotConfig cfg = new FusionBotConfig();
        public IBotConfig Config => cfg;

        public Task ClientReady() => Task.CompletedTask;

        public void Init(CommandLineParser cmd)
        {
            if (cmd["port"] != null)
				cfg.WalletPort = uint.Parse(cmd["port"].Value);

			string walletFile = string.Empty, walletPassword = string.Empty;
			string keyFilePassword = null;

			if (cmd["key-file"] != null)
			{
				string[] keys = File.ReadAllLines(cmd["key-file"].Value);
				keyFilePassword = PasswordPrompt.Get("Please enter the key file decryption password");

				walletPassword = keys[0].Decrypt(keyFilePassword);
				cfg.DonationPaymentIdKey = keys[1].Decrypt(keyFilePassword);

				keyFilePassword = null;
			}
			else
			{
				walletPassword = PasswordPrompt.Get("Please enter the donation wallet password");
				cfg.DonationPaymentIdKey = PasswordPrompt.Get("Please enter the payment id encryption key");
			}

			if (cmd["wallet"] != null)
				walletFile = cmd["wallet"].Value;

			string jsonFile = Path.Combine(Environment.CurrentDirectory, $"Wallets/{walletFile}.json");
			Log.Write($"Loading Wallet JSON: {jsonFile}");
			cfg.AccountJson = JsonConvert.DeserializeObject<AccountJson>(File.ReadAllText(jsonFile));

			new OpenWallet(new OpenWalletRequestData {
				FileName = walletFile,
				Password = walletPassword
			}, (string result) => {
				Log.Write("Wallet loaded");
			}, (RequestError error) => {
				Log.Write("Failed to load wallet");
				Environment.Exit(1);
			}, cfg.WalletHost, cfg.WalletPort).Run();
        }
    }
}