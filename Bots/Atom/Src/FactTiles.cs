using System;
using System.IO;

namespace Atom
{
    public static class FactTiles
    {
        public const string URL = "https://getnerva.org/content/quotes";

        private static int current = 0;
        private static Random rand = new Random();

        public static readonly ulong[] SERVERS = new ulong[]
        {
            439649936955408396, //General
            439656270589329408, //Mining
            439656297156313088, //Trading
        };

        private const int TOTAL_TILES = 82;

        public static string GetImage()
        {
            string iPath = Path.Combine(Environment.CurrentDirectory, "tile");

            if (File.Exists(iPath))
                current = int.Parse(File.ReadAllText(iPath).Trim());

            if (current > TOTAL_TILES)
                current = 1;

            string url = $"{URL}/{current}.png";
            ++current;
            
            File.WriteAllText(iPath, current.ToString());

            return url;
        }

        public static string GetRandom(out int r)
        {
            r = rand.Next(1, TOTAL_TILES);
            return $"{URL}/{r}.png";
        }
    }
}