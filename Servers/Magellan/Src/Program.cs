using System;
using System.IO;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Logger;

namespace MagellanServer
{
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Log.CreateInstance(true);
            AngryWasp.Serializer.Serializer.Initialize();

            CommandLineParser cmd = CommandLineParser.Parse(args);

            if (File.Exists("NodeMap.xml"))
            {
                Log.Instance.Write("Loading node map info");
                NodeMapDataStore.Load();
            }

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
				Environment.Exit(0);
			}
			else
				Log.Instance.Write($"Loaded Geolocation API key {apiKey}");

            GeoLocator.ApiKey = apiKey;

            new RpcListener().Start();

            Task.Delay(0);
        }
    }
}