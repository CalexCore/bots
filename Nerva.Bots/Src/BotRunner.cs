using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Discord;
using Discord.WebSocket;
using Nerva.Bots.Commands;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Log_Severity = AngryWasp.Logger.Log_Severity;

namespace Nerva.Bots
{
    public class BotRunner
    {
        [STAThread]
		public static void Main(string[] args)
		{
			try	
			{
				new BotRunner().MainAsync(args).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				Log.Write(Log_Severity.Fatal, ex.Message + "\r\n" + ex.StackTrace);
				Environment.Exit(0);
			}
		}
			
		public async Task MainAsync(string[] args)
		{
            CommandLineParser cmd = CommandLineParser.Parse(args);
            AngryWasp.Logger.Log.CreateInstance(true);

            string token = null;
            string botAssembly = null;
			Globals.DevMode = cmd["dev-mode"] != null;
			
			if (cmd["token"] != null)
			{
				await Log.Write("Token loaded from command line");
				token = cmd["token"].Value;
			}

			if (token == null && cmd["token-file"] != null)
			{
				await Log.Write("Token loaded from file");
				token = File.ReadAllText(cmd["token-file"].Value);
			}

			if (token == null)
			{
				await Log.Write(Log_Severity.Fatal, "Bot token not provided!");
				Environment.Exit(0);
			}
			else
				await Log.Write($"Loaded token {token}");

			string pw = null;
			string decryptedToken = null;

            if (cmd["password"] != null)
                pw = cmd["password"].Value;
            else
                pw = PasswordPrompt.Get("Please enter your token decryption password");

			try {
				decryptedToken = token.Decrypt(pw);
			} catch {
				await Log.Write(Log_Severity.Fatal, "Incorrect password");
				Environment.Exit(0);
			}

			await Log.Write($"Dev Mode: {Globals.DevMode}");

			if (cmd["bot"] == null)
			{
				await Log.Write(Log_Severity.Fatal, "Bot plugin not provided!");
				Environment.Exit(0);
			}
			else
				botAssembly = cmd["bot"].Value;

			if (!File.Exists(botAssembly))
			{
				await Log.Write(Log_Severity.Fatal, "Bot plugin bot found!");
				Environment.Exit(0);
			}

			if (cmd["debug"] != null)
				Globals.RpcLogConfig = Nerva.Rpc.Log.Presets.Normal;

			List<int> errorCodes = new List<int>();

			for (int i = 0; i < cmd.Count; i++)
			{
				if (cmd[i].Flag == "debug-hide")
				{
					int j = 0;
					if (int.TryParse(cmd[i].Value, out j))
						errorCodes.Add(-j);
				}
			}

			if (errorCodes.Count > 0)
				Globals.RpcLogConfig.SuppressRpcCodes = errorCodes;

            //load plugin
            Globals.BotAssembly = ReflectionHelper.Instance.LoadAssemblyFile(botAssembly);
			Type botPluginType = ReflectionHelper.Instance.GetTypesInheritingOrImplementing(Globals.BotAssembly, typeof(IBot))[0];
			Globals.Bot = (IBot)Activator.CreateInstance(botPluginType);

			List<Type> botCommands = ReflectionHelper.Instance.GetTypesInheritingOrImplementing(Globals.BotAssembly, typeof(ICommand));

			//common commands for all bots
			botCommands.Add(typeof(Help));
			botCommands.Add(typeof(Ping));

			Globals.Bot.Init(cmd);

			var client = new DiscordSocketClient();
			Globals.Client = client;
			client.Log += Log.Write;

			await client.LoginAsync(TokenType.Bot, decryptedToken);
			await client.StartAsync();

			foreach (Type t in botCommands)
			{
				CommandAttribute ca = t.GetCustomAttribute(typeof(CommandAttribute)) as CommandAttribute;
				if (ca == null)
					continue;

				Globals.BotHelp.Add($"{Globals.Bot.Config.CmdPrefix}{ca.Cmd}", ca.Help);
				Globals.Commands.Add($"{Globals.Bot.Config.CmdPrefix}{ca.Cmd}", ((ICommand)Activator.CreateInstance(t)).Process);
			}

			client.MessageReceived += MessageReceived;
			client.Ready += ClientReady;

			await Task.Delay(-1);
        }

		private async Task ClientReady()
		{
			await Globals.Bot.ClientReady();
		}

		private async Task MessageReceived(SocketMessage message)
		{
			var msg = message as SocketUserMessage;
            if (msg == null)
                return;

            Regex pattern = new Regex($@"\{Globals.Bot.Config.CmdPrefix}\w+");
            var commands = pattern.Matches(msg.Content.ToLower()).Cast<Match>().Select(match => match.Value).ToArray();

            if (commands.Length == 0)
                return;

			foreach (var c in commands)
            {
                if (Globals.Commands.ContainsKey(c))
                    await Task.Run(() => Globals.Commands[c].Invoke(msg));
            }
		}
    }

    public static class Globals
    {
        public static bool DevMode { get; set; } = false;

        public static Assembly BotAssembly { get; set; } = null;

		public static IBot Bot { get; set; } = null;

        public static DiscordSocketClient Client { get; set; }

		public static Nerva.Rpc.Log RpcLogConfig { get; set; } = Nerva.Rpc.Log.Presets.None;

		public static Dictionary<string, string> BotHelp { get; set; } = new Dictionary<string, string>();

		public static Dictionary<string, Action<SocketUserMessage>> Commands = new Dictionary<string, Action<SocketUserMessage>>();
    }
}