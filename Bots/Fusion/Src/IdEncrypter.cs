using System;
using System.Text;
using System.Linq;
using AngryWasp.Helpers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Nerva.Bots;

namespace Fusion
{
    public static class IdEncrypter
    {
        private const int BLOCK_SIZE = 16;

        public static string Encrypt(ulong sender, ulong recipient)
        {
            var mt = new MersenneTwister((uint)Guid.NewGuid().GetHashCode());
            var key = Encoding.UTF8.GetBytes(((FusionBotConfig)Globals.Bot.Config).DonationPaymentIdKey);
            
            if (sender == 0)
                sender = mt.NextULong();

            if (recipient == 0)
                recipient = mt.NextULong();

            var iv = mt.NextBytes(BLOCK_SIZE);
            var data = BitShifter.ToByte(sender).Concat(BitShifter.ToByte(recipient)).ToArray();

            BufferedBlockCipher cipher = new CtsBlockCipher(new CbcBlockCipher(new AesEngine()));
            ICipherParameters keyParam = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(true, keyParam);
            return iv.Concat(cipher.DoFinal(data, 0, data.Length)).ToArray().ToHex();
        }

        public static void Decrypt(string hexData, out ulong sender, out ulong recipient)
        {
            sender = 0;
            recipient = 0;

            if (hexData.Length != 64)
                return;

            var key = Encoding.UTF8.GetBytes(((FusionBotConfig)Globals.Bot.Config).DonationPaymentIdKey);
            var data = hexData.FromByteHex();
            var iv = data.Take(BLOCK_SIZE).ToArray();
            data = data.Skip(BLOCK_SIZE).ToArray();

            BufferedBlockCipher cipher = new CtsBlockCipher(new CbcBlockCipher(new AesEngine()));
            ICipherParameters keyParam = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(false, keyParam);
            var result = cipher.DoFinal(data, 0, data.Length);

            int start = 0;
            sender = BitShifter.ToULong(result, ref start);
            recipient = BitShifter.ToULong(result, ref start);
        }
    }
}