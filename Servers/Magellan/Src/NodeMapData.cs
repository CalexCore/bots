using System.Collections.Generic;
using System.Xml.Linq;
using AngryWasp.Logger;
using AngryWasp.Serializer;
using Newtonsoft.Json;

namespace MagellanServer
{
    [JsonObject]
    public class RpcRequest<T>
    {
        [JsonProperty("method")]
        public string Method { get; set; } = string.Empty;

        [JsonProperty("params")]
        public T Params { get; set; }
    }

    public class NodeMapEntry
    {
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;

        [JsonProperty("time")]
        public ulong LastAccessTime { get; set; } = 0;

        public float Latitude { get; set; } = 0;

        public float Longitude { get; set; } = 0;
    }

    public class NodeMapDataStore
    {
        private static Dictionary<string, NodeMapEntry> nodeMap = new Dictionary<string, NodeMapEntry>();
        
        public static bool Add(NodeMapEntry e)
        {
            //The node map data is keyed by the hash of the ip address. So we need to first get that
            string hash = e.Address.GetHashString();

            //if ip is already in the node map, update the last access time
            if (nodeMap.ContainsKey(hash))
            {
                Log.Instance.Write("Node map entry updated");
                nodeMap[hash].LastAccessTime = e.LastAccessTime;
            }
            else
            {
                //if not already in the map, we need to locate the ip
                //we only add the node to the map if geolocation was successful
                //most likely reason for failure is exceeding the daily API rate limit
                float latitude = 0, longitude = 0;
                if (GeoLocator.Get(e.Address, out latitude, out longitude))
                {
                    e.Address = hash;
                    e.Latitude = latitude;
                    e.Longitude = longitude;
                    nodeMap.Add(hash, e);
                    Log.Instance.Write("Node map entry created");
                }
                else
                    return false;
            }

            new ObjectSerializer().Serialize(nodeMap, "NodeMap.xml");
            Log.Instance.Write("Node map data saved");
            return true;
        }

        public static void Load()
        {
            nodeMap = new ObjectSerializer().Deserialize<Dictionary<string, NodeMapEntry>>(XDocument.Load("NodeMap.xml"));
            Log.Instance.Write("Node map data loaded");
        }
    }
}