using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    public class SubmitParams
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

    public class FetchParams
    {
        [JsonProperty("time")]
        public ulong Time { get; set; } = 0;

        [JsonProperty("limit")]
        public ulong Limit { get; set; } = 0;
    }

    public class PruneParams
    {
        [JsonProperty("time")]
        public ulong Time { get; set; } = 0;

        [JsonProperty("limit")]
        public ulong Limit { get; set; } = 0;

        [JsonProperty("dryrun")]
        public bool DryRun { get; set; } = true;

        [JsonProperty("key")]
        public string Key { get; set; } = string.Empty;
    }

    public class NodeMapDataStore
    {
        [SerializerInclude]
        private Dictionary<string, SubmitParams> nodeMap = new Dictionary<string, SubmitParams>();

        public Dictionary<string, SubmitParams> NodeMap => nodeMap;

        public bool Add(SubmitParams e, out bool newEntryCreated)
        {
            string hash = e.Address.GetHashString();
            newEntryCreated = false;

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
                    newEntryCreated = true;
                }
                else
                    return false;
            }

            return true;
        }

        public bool Prune(PruneParams pp, out int prunedCount)
        {
            ulong limit = pp.Time - (86400 * pp.Limit);

            prunedCount = 0;

            try
            {
                if (pp.DryRun)
                {
                    foreach (var n in nodeMap.Values)
                        if (n.LastAccessTime < limit)
                            ++prunedCount;

                    Log.Instance.Write($"{prunedCount} nodes older than {pp.Limit} days");
                }
                else
                {
                    Dictionary<string, SubmitParams> prunedMap = new Dictionary<string, SubmitParams>();

                    foreach (var n in nodeMap)
                        if (n.Value.LastAccessTime < limit)
                            ++prunedCount;
                        else
                            prunedMap.Add(n.Key, n.Value);

                    nodeMap = prunedMap;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Instance.WriteNonFatalException(ex);
                return false;
            }
        }

        public string Fetch(FetchParams fp)
        {
            if (File.Exists("/var/www/html/nodemap.json"))
            {
                Log.Instance.Write("Fetching from text file");
                return File.ReadAllText("/var/www/html/nodemap.json");
            }
            
            ulong limit = fp.Time - (86400 * fp.Limit);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");

            SubmitParams[] n = nodeMap.Values.ToArray();

            for (int i = 0; i < n.Length ; i++)
            {
                if (n[i].LastAccessTime >= limit)
                {
                    if (i < n.Length - 1)
                        sb.AppendLine(NodeMapEntryToJson(n[i]) + ",");
                    else
                        sb.AppendLine(NodeMapEntryToJson(n[i]));
                }
            }

            sb.AppendLine("]");

            return sb.ToString();
        }

        public string FetchAll()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");

            SubmitParams[] n = nodeMap.Values.ToArray();

            for (int i = 0; i < n.Length ; i++)
            {
                if (i < n.Length - 1)
                    sb.AppendLine(NodeMapEntryToJson(n[i]) + ",");
                else
                    sb.AppendLine(NodeMapEntryToJson(n[i]));    
            }

            sb.AppendLine("]");

            return sb.ToString();
        }

        private string NodeMapEntryToJson(SubmitParams n) =>
            $"{{\"version\":\"{n.Version}\",\"time\":\"{n.LastAccessTime}\",\"lat\":\"{n.Latitude}\",\"long\":\"{n.Longitude}\",\"cn\":\"{n.ContinentCode}\",\"cc\":\"{n.CountryCode}\"}}";
    }
}
