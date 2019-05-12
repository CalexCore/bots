using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;
using AngryWasp.Serializer.Serializers;

namespace MagellanServer
{
    public class MainClass
    {
        private const int DEFAULT_PORT = 15236;

        [STAThread]
        public static void Main(string[] args)
        {
            Log.CreateInstance(true);
            AngryWasp.Serializer.Serializer.Initialize();

            CommandLineParser cmd = CommandLineParser.Parse(args);

            NodeMapDataStore ds = new NodeMapDataStore();

            if (File.Exists("NodeMap.xml"))
            {
                Log.Instance.Write("Loading node map info");
                ds = new ObjectSerializer().Deserialize<NodeMapDataStore>(XDocument.Load("NodeMap.xml"));
                Log.Instance.Write($"Node map loaded {ds.NodeMap.Count} items from file");
            }

            string apiKey = null;
            List<string> allowedKeys = new List<string>();

            if (cmd["geo-api-key"] != null)
			{
				Log.Instance.Write("Geolocation API key loaded from command line");
				apiKey = cmd["geo-api-key"].Value;
			}

			if (apiKey == null && cmd["geo-api-key-file"] != null)
			{
                //todo: check file exists
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

            if (cmd["access-keys"] != null)
			{
                //todo: check file exists
                Log.Instance.Write("Access keys loaded from file");
				File.ReadAllText(cmd["access-keys"].Value);
			}    

            GeoLocator.ApiKey = apiKey;

            int port = DEFAULT_PORT;

            if (cmd["port"] != null)
				port = new IntSerializer().Deserialize(cmd["port"].Value);

            Log.Instance.Write($"Listening on port {port}");
            new RpcListener().Start(ds, port);

            Task.Delay(0);
        }
    }
}