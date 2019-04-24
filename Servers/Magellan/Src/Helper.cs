using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngryWasp.Logger;
using IPGeolocation;
using Newtonsoft.Json.Linq;

namespace MagellanServer
{
    //todo: move to string helpers
    public static class ShaHash
    {
        public static byte[] GetHash(this string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(this string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in inputString.GetHash())
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }

    public static class GeoLocator
    {
        public static string ApiKey { get; set; } = null;

        public static bool Get(string ip, out float latitude, out float longitude)
        {
            latitude = longitude = 0;
            if (string.IsNullOrEmpty(ApiKey))
            {
                Log.Instance.Write(Log_Severity.Error, "Geolocation API key not provided");
                return false;
            }

            if (!Regex.IsMatch(ip, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$"))
            {
                Log.Instance.Write(Log_Severity.Error, "IP address has incorrect format");
                return false;
            }

            try
            {
                IPGeolocationAPI api = new IPGeolocationAPI(ApiKey);

                GeolocationParams gp = new GeolocationParams();
                gp.SetIPAddress(ip);
                gp.SetFields("latitude,longitude");

                Geolocation locate = api.GetGeolocation(gp);

                if (locate.GetStatus() == 200)
                {
                    if (!float.TryParse(locate.GetLatitude(), out latitude) ||
                        !float.TryParse(locate.GetLongitude(), out longitude))
                    {
                        Log.Instance.Write(Log_Severity.Error, "Failed to parse latitude and/or longituide");
                        return false;
                    }

                    return true;
                }
                else
                {
                    Log.Instance.Write(Log_Severity.Error, $"Geolocation returned error: {locate.GetMessage()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Error, $"Geolocation threw exception: {ex.Message}");
                return false;
            }
        }
    }
}