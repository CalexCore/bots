using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;
using MagellanServer;
using Nerva.Rpc.Daemon;
using Newtonsoft.Json;

namespace Tools
{
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineParser cmd = CommandLineParser.Parse(args);
            Log.CreateInstance(true);
            Serializer.Initialize();

            if (!GetApiKeyArg(cmd))
                Environment.Exit(1);

            string url = cmd["url"].Value;

            string returnString, errorString;
            if (!NetHelper.HttpRequest(url, out returnString, out errorString))
            {
                Log.Instance.Write(Log_Severity.Error, errorString);
                return;
            }

            GetPeerListResponseData peerList = JsonConvert.DeserializeObject<GetPeerListResponseData>(returnString);

            NodeMapDataStore nmap = new NodeMapDataStore();

            if (File.Exists("NodeMap.xml"))
            {
                Log.Instance.Write("Loading node map data from file");
                nmap = new ObjectSerializer().Deserialize<NodeMapDataStore>(XDocument.Load("NodeMap.xml"));
            }

            RunList(peerList.GrayList, ref nmap);
            RunList(peerList.WhiteList, ref nmap);

            new ObjectSerializer().Serialize(nmap, "NodeMap.xml");
            Log.Instance.Write($"DONE! {nmap.NodeMap.Count} nodes in map");
        }

        private static void RunList(List<GetPeerListResponseDataItem> list, ref NodeMapDataStore nmap)
        {
            foreach (var p in list)
            {
                string ip = GetIpFromInteger(p.IP);
                string hash = ip.GetHashString();

                if (nmap.NodeMap.ContainsKey(hash) || p.IP == 0)
                {
                    Log.Instance.Write($"Skipping IP: {ip}");
                    continue;
                }

                DateTime now = DateTime.UtcNow;
                DateTime ls = TimeStampToDateTime(p.LastSeen);

                int age = (now - ls).Days;

                if (age > 14)
                {
                    Log.Instance.Write($"Skipping Stale IP: {ip}. {age} days old");
                    continue;
                }
                
                ulong ts = RoundTimestamp(p.LastSeen);
                SubmitParams e = new SubmitParams();
                e.Address = ip;
                e.LastAccessTime = ts;
                e.Version = "0.0.0.0";

                if (nmap.Add(e))
                    Log.Instance.Write($"Added IP: {ip}");
            }
        }

        private static string GetIpFromInteger(uint ipInt)
        {
            var b = BitShifter.ToByte(ipInt);
            return $"{b[0]}.{b[1]}.{b[2]}.{b[3]}";
        }

        public static DateTime TimeStampToDateTime(ulong ts)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime dt = epoch.AddSeconds(ts).ToLocalTime();
            return dt;
        }

        public static ulong RoundTimestamp(double ts)
        {
            //convert input TimeStamp to a DateTime
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime dt = epoch.AddSeconds(ts).ToLocalTime();

            //round back to midnight by creating a new DateTime using the dt date with a time of 00:00
            var midnight = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, System.DateTimeKind.Utc);
            return (ulong)(midnight - epoch).TotalSeconds;
        }

        private static bool GetApiKeyArg(CommandLineParser cmd)
        {
            string apiKey = null;

            if (cmd["geo-api-key"] != null)
			{
				Log.Instance.Write("Geolocation API key loaded from command line");
				apiKey = cmd["geo-api-key"].Value;
			}

			if (apiKey == null && cmd["geo-api-key-file"] != null)
			{
				Log.Instance.Write("Geolocation API key loaded from file");
				apiKey = File.ReadAllText(cmd["geo-api-key-file"].Value);
			}

			if (apiKey == null)
			{
				Log.Instance.Write(Log_Severity.Fatal, "Geolocation API key not provided!");
				return false;
			}
			else
            {
				Log.Instance.Write($"Loaded Geolocation API key {apiKey}");
                GeoLocator.ApiKey = apiKey;
                return true;
            }
        }
    }
}