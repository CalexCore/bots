using System;

namespace Nerva.Bots.Helpers
{
    public static class Conversions
    {
        public static double FromAtomicUnits(this ulong i) => Math.Round((double)i / 1000000000000.0d, 4);

        public static ulong ToAtomicUnits(this double i) => (ulong)(i * 1000000000000.0d);

        public static string ToMention(this ulong id)
        {
            string user = "unknown";

            var su = Globals.Client.GetUser(id);
            if (su != null)
                user = su.Mention;

            return user;
        } 
    }
}