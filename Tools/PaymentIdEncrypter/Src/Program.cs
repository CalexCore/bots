using System;
using System.Text;
using System.Linq;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Log = AngryWasp.Logger.Log;

namespace Tools
{
    public class MainClass
    {
        private const int BLOCK_SIZE = 16;
        
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineParser cmd = CommandLineParser.Parse(args);
            Log.CreateInstance(true);

            string keyText = null;

            if (cmd["key"] != null)
                keyText = cmd["key"].Value;
            else
            {
                keyText = PasswordPrompt.Get("Please enter the encryption key");
                string keyText2 = PasswordPrompt.Get("Please confirm the encryption key");

                if (keyText != keyText2)
                    Log.Instance.Write(Log_Severity.Fatal, "Keys did not match");
            }
            
            if (cmd["to"] == null || cmd["from"] == null) 
                Log.Instance.Write(Log_Severity.Fatal, "Need arguments 'to' and 'from'");

            ulong sender = ulong.Parse(cmd["from"].Value);
            ulong recipient = ulong.Parse(cmd["to"].Value);

            var mt = new MersenneTwister((uint)Guid.NewGuid().GetHashCode());
            var key = Encoding.UTF8.GetBytes(keyText);
            
            if (sender == 0)
                sender = mt.NextULong();

            if (recipient == 0)
                recipient = mt.NextULong();

            var iv = mt.NextBytes(BLOCK_SIZE);
            var data = BitShifter.ToByte(sender).Concat(BitShifter.ToByte(recipient)).ToArray();

            BufferedBlockCipher cipher = new CtsBlockCipher(new CbcBlockCipher(new AesEngine()));
            ICipherParameters keyParam = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(true, keyParam);
            Log.Instance.Write(iv.Concat(cipher.DoFinal(data, 0, data.Length)).ToArray().ToHex());
        }
    }
}