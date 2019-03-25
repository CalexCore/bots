using System;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Nerva.Bots.Helpers;
using Log = AngryWasp.Logger.Log;

namespace Tools
{
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineParser cmd = CommandLineParser.Parse(args);
            Log.CreateInstance(true);

            string pw = null;

            if (cmd["password"] != null)
                pw = cmd["password"].Value;
            else
            {
                pw = PasswordPrompt.Get();
                string pw2 = PasswordPrompt.Get("Please confirm your password");

                if (pw != pw2)
                    Log.Instance.Write(Log_Severity.Fatal, "Passwords did not match");
            }

            if (cmd["encrypt"] != null)
            {
                try {
                    Log.Instance.Write(cmd["encrypt"].Value.Encrypt(pw));
                } catch {
                    Log.Instance.Write(Log_Severity.Fatal, "Encryption failed");
                }
            }

            if (cmd["decrypt"] != null)
            {
                try {
                    Log.Instance.Write(cmd["decrypt"].Value.Decrypt(pw));
                } catch {
                    Log.Instance.Write(Log_Severity.Fatal, "Decryption failed");
                }
            }
        }
    }
}