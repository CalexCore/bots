using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
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

            string dataDir = null;

            if (cmd["data-dir"] != null)
                dataDir = cmd["data-dir"].Value;
            else
                dataDir = Environment.CurrentDirectory;

            Log.Instance.Write($"Using data directory {dataDir}");

            if (File.Exists(Path.Combine(dataDir, "NodeMap.xml")))
            {
                Log.Instance.Write("Loading node map info");
                ds = new ObjectSerializer().Deserialize<NodeMapDataStore>(XDocument.Load(Path.Combine(dataDir, "NodeMap.xml")));
                Log.Instance.Write($"Node map loaded {ds.NodeMap.Count} items from file");

                if (!File.Exists("/var/www/html/nodemap.json"))
                    File.WriteAllText("/var/www/html/nodemap.json", $"{{\"status\":\"OK\",\"result\":{ds.FetchAll()}}}\r\n");
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
				Config.AllowedKeys = File.ReadAllLines(cmd["access-keys"].Value).ToList();
			}    

            GeoLocator.ApiKey = apiKey;

            int port = DEFAULT_PORT;

            if (cmd["port"] != null)
				port = new IntSerializer().Deserialize(cmd["port"].Value);

            Log.Instance.Write($"Listening on port {port}");

            RpcListener r = new RpcListener();

            bool mapDataChanged = false;

            r.MapDataChanged += () => {
                mapDataChanged = true;
            };

            //run once every 5 minutes
            Timer t = new Timer(1000 * 60 * 5);
            t.Elapsed += (s, e) =>
            {
                if (mapDataChanged)
                {
                    Task.Run( () =>
                    {
                        new ObjectSerializer().Serialize(r.DataStore, Path.Combine(dataDir, "NodeMap.xml"));
                        Log.Instance.Write("Node map data saved");

                        Log.Instance.Write("Saving node map data to json");
                        File.WriteAllText("/var/www/html/nodemap.json", $"{{\"status\":\"OK\",\"result\":{r.DataStore.FetchAll()}}}\r\n");

                        mapDataChanged = false;
                    });
                }
            };

            t.Start();

            r.Start(ds, port);

            Application.Start();
        }
    }
}