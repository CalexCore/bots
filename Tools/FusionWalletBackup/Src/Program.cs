using System;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using AngryWasp.Helpers;
using Log = AngryWasp.Logger.Log;
using System.Collections.Generic;
using System.IO;

namespace Tools
{
    //to use this tool, open the wallet in an rpc instance first
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineParser cmd = CommandLineParser.Parse(args);
            Log.CreateInstance(true);

            if (cmd["backup"] != null && cmd["restore"] != null)
                Log.Instance.Write(AngryWasp.Logger.Log_Severity.Fatal, "Cannot specify both 'backup' and 'restore'");

            if (cmd["port"] == null)
                Log.Instance.Write(AngryWasp.Logger.Log_Severity.Fatal, "Need a wallet port");

            uint port = uint.Parse(cmd["port"].Value);

            string filePath = cmd["file"] == null ? $"fusion-wallet.backup.{DateTimeHelper.TimestampNow}" : cmd["file"].Value;

            if (cmd["backup"] != null)
            {
                new GetAccounts(null, (GetAccountsResponseData r) =>
                {
                    List<string> outputFileContents = new List<string>();

                    foreach (var a in r.Accounts)
                    {
                        ulong id = 0;
                        ulong.TryParse(a.Label, out id);

                        Log.Instance.Write(AngryWasp.Logger.Log_Severity.None, $"{a.Index.ToString().PadLeft(3)}: {id}");
                        outputFileContents.Add(id.ToString());
                    }

                    File.WriteAllLines(filePath, outputFileContents);
                },
                (RequestError error) =>
                {
                    Log.Instance.Write($"Failed to load wallet: {error.Message}");
                    Environment.Exit(1);
                }, "127.0.0.1", port).Run();
            }
            else if (cmd["restore"] != null)
            {
                if (cmd["file"] == null)
                    Log.Instance.Write(AngryWasp.Logger.Log_Severity.Fatal, "Need a file to restore from");

                string[] entries = File.ReadAllLines(cmd["file"].Value);
                List<ulong> parsed = new List<ulong>();

                foreach (string entry in entries)
                {
                    ulong id = 0;
                    if (!ulong.TryParse(entry, out id))
                        Log.Instance.Write(AngryWasp.Logger.Log_Severity.Fatal, "Account label is not a valid Discord ID. Backup is corrupt");

                    parsed.Add(id);
                }

                if (entries.Length != parsed.Count)
                    Log.Instance.Write(AngryWasp.Logger.Log_Severity.Fatal, "Account labels not parsed correctly. Backup is corrupt");

                for (int i = 0; i < parsed.Count; i++)
                {
                    ulong u = parsed[i];
                    if (u == 0)
                    {
                         Log.Instance.Write($"Wallet index {i}: Skipped");
                        continue;
                    }

                    new LabelAccount(new LabelAccountRequestData
                    {
                        Index = (uint)i,
                        Label = u.ToString()
                    }, (string r) =>
                    {
                        Log.Instance.Write($"Wallet index {i}: Restored");
                    }, (RequestError e) =>
                    {
                        Log.Instance.Write(AngryWasp.Logger.Log_Severity.Fatal, "Setting account label failed. Restored wallet corrupted.");
                    }, "127.0.0.1", port).Run(); 
                }

                 Log.Instance.Write("Restore complete");
            }
            else
                Log.Instance.Write(AngryWasp.Logger.Log_Severity.Fatal, "Need to specify either 'backup' or 'restore'");
        }
    }
}