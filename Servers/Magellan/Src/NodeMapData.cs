using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        [SerializerExclude]
        public string Address { get; set; } = string.Empty;

        [JsonProperty("time")]
        public ulong LastAccessTime { get; set; } = 0;

        public float Latitude { get; set; } = 0;

        public float Longitude { get; set; } = 0;

        public string ContinentCode { get; set; } = string.Empty;

        public string CountryCode { get; set; } = string.Empty;
    }

    public class NodeMapDataStore
    {
        [SerializerInclude]
        private Dictionary<string, NodeMapEntry> nodeMap = new Dictionary<string, NodeMapEntry>();

        public Dictionary<string, NodeMapEntry> NodeMap => nodeMap;
        
        public bool Add(NodeMapEntry e)
        {
            string hash = e.Address.GetHashString();

            if (nodeMap.ContainsKey(hash))
            {
                Log.Instance.Write("Node map entry updated");
                nodeMap[hash].LastAccessTime = e.LastAccessTime;
                nodeMap[hash].Version = e.Version;
            }
            else
            {
                float latitude = 0, longitude = 0;
                string continentCode, countryCode;
                if (GeoLocator.Get(e.Address, out latitude, out longitude, out continentCode, out countryCode))
                {
                    e.Address = hash;
                    e.Latitude = latitude;
                    e.Longitude = longitude;
                    e.ContinentCode = continentCode;
                    e.CountryCode = countryCode;
                    nodeMap.Add(hash, e);
                    Log.Instance.Write("Node map entry created");
                }
                else
                    return false;
            }

            return true;
        }
    
        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");

            NodeMapEntry[] na = nodeMap.Values.ToArray();

            for (int i = 0; i < na.Length -1; i++)
            {
                sb.Append(NodeMapEntryToJson(na[i]));
                sb.AppendLine(",");
            }

            sb.AppendLine(NodeMapEntryToJson(na[na.Length - 1]));

            sb.Append("]");

            return sb.ToString();
        }

        private string NodeMapEntryToJson(NodeMapEntry n) =>
            $"{{\"version\":\"{n.Version}\",\"time\":\"{n.LastAccessTime}\",\"lat\":\"{n.Latitude}\",\"long\":\"{n.Longitude}\",\"cn\":\"{n.ContinentCode}\",\"cc\":\"{n.CountryCode}\"}}";
    }
}