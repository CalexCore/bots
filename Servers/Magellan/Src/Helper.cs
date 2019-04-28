using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngryWasp.Logger;
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
        public static bool HttpRequest(string ip, out string returnString)
        {
            string url = $"https://api.ipgeolocation.io/ipgeo?apiKey={ApiKey}&ip={ip}&fields=latitude,longitude,continent_code,country_code2";

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                using (Stream stream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    returnString = reader.ReadToEnd();
                }
                    
                return true;
            }
            catch (Exception ex)
            {
                Log.Instance.Write($"Error attempting HTTP request {url}");
                Log.Instance.WriteNonFatalException(ex);
                returnString = null;
                return false;
            }
        }

        public static string ApiKey { get; set; } = null;

        public static bool Get(string ip, out float latitude, out float longitude, out string continentCode, out string countryCode)
        {
            latitude = longitude = 0;
            continentCode = countryCode = "-";
            if (string.IsNullOrEmpty(ApiKey))
            {
                Log.Instance.Write(Log_Severity.Error, "Geolocation API key not provided");
                return false;
            }

            if (!Regex.IsMatch(ip, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$"))
            {
                Log.Instance.Write(Log_Severity.Error, $"IP address has incorrect format: {ip}");
                return false;
            }

            try
            {
                string returnJson;
                HttpRequest(ip, out returnJson);

                dynamic result = JObject.Parse(returnJson);

                if (result.ip == null || result.ip.Value != ip)
                {
                    Log.Instance.Write(Log_Severity.Error, "Returned IP does not match requested");
                    return false;
                }

                if (result.latitude == null || result.longitude == null)
                {
                    Log.Instance.Write(Log_Severity.Error, "Latitude and/or longitude is null");
                    return false;
                }

                if (!float.TryParse(result.latitude.Value, out latitude) ||
                    !float.TryParse(result.longitude.Value, out longitude))
                {
                    Log.Instance.Write(Log_Severity.Error, "Failed to parse latitude and/or longituide");
                    return false;
                }

                if (result.continent_code != null && !string.IsNullOrEmpty(result.continent_code.Value))
                    continentCode = result.continent_code.Value;

                if (result.country_code2 != null && !string.IsNullOrEmpty(result.country_code2.Value))
                    countryCode = result.country_code2.Value;

                return true;

            }
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Error, $"Geolocation threw exception: {ex.Message}");
                return false;
            }
        }
    }
}