using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AngryWasp.Serializer;

namespace MagellanServer
{
    public class RpcListener
    {
        private NodeMapDataStore dataStore = new NodeMapDataStore();

        public void Start(NodeMapDataStore ds, int port)
        {
            dataStore = ds;
            TcpListener listener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
            listener.Start();
            Log.Instance.Write("Local endpoint: " + listener.LocalEndpoint);
            Log.Instance.Write("RPC server initialized");

            while (true)
            {
                if (listener.Pending())
                {
                    var c = listener.AcceptTcpClient();
                    c.NoDelay = true;
                    NetworkStream ns = c.GetStream();
                    byte[] bytes = new byte[256];
                    int i = 0;
                    StringBuilder sb = new StringBuilder();

                    while ((i = ns.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        sb.Append(Encoding.ASCII.GetString(bytes, 0, i));
                        if (!ns.DataAvailable)
                            break;
                    }

                    try
                    {
                        //the http standard seperates header and content by a double line break
                        //so we just take a substring from the first instance of that
                        string sbs = sb.ToString();
                        string content = sbs.Substring(sbs.IndexOf("\r\n\r\n")).Trim();

                        dynamic json = JObject.Parse(content);
                        bool ok = false;
                        string resultData = null;

                        if (json.method == null)
                            Log.Instance.Write(Log_Severity.Warning, "No JSON method provided");
                        else
                        {
                            Log.Instance.Write($"Received new request: {json.method}");
                            switch (json.method.Value)
                            {
                                case "submit":
                                    {
                                        SubmitParams sp = JsonConvert.DeserializeObject<RpcRequest<SubmitParams>>(content).Params;
                                        ok = dataStore.Add(sp);
                                        if (ok)
                                        {
                                            new ObjectSerializer().Serialize(dataStore, "NodeMap.xml");
                                            Log.Instance.Write("Node map data saved");
                                        }
                                    }
                                    break;
                                case "fetch":
                                    {
                                        FetchParams fp = JsonConvert.DeserializeObject<RpcRequest<FetchParams>>(content).Params;
                                        resultData = dataStore.Fetch(fp);
                                        ok = true;
                                    }
                                    break;
                                case "prune":
                                    {
                                        if (json.key == null)
                                        {
                                            Log.Instance.Write(Log_Severity.Warning, "No access key provided");
                                            ok = false;
                                            break;
                                        }

                                        if (Config.AllowedKeys.Contains(json.Key))
                                        {
                                            Log.Instance.Write(Log_Severity.Warning, "Invalid access key");
                                            ok = false;
                                            break;
                                        }

                                        PruneParams pp = JsonConvert.DeserializeObject<RpcRequest<PruneParams>>(content).Params;
                                        ok = dataStore.Prune(pp);
                                        if (ok && !pp.DryRun)
                                        {
                                            new ObjectSerializer().Serialize(dataStore, "NodeMap.xml");
                                            Log.Instance.Write("Node map data saved");
                                        }
                                    }
                                    break;
                                default:
                                    Log.Instance.Write($"Invalid request: {json.Method}");
                                    break;
                            }
                        }

                        if (ok)
                        {
                            if (resultData == null)
                                ns.Write(Encoding.ASCII.GetBytes("{\"status\":\"OK\"}\r\n"));
                            else
                                ns.Write(Encoding.ASCII.GetBytes($"{{\"status\":\"OK\",\"result\":{resultData}}}\r\n"));
                        }
                        else
                            ns.Write(Encoding.ASCII.GetBytes("{\"status\":\"ERROR\"}\r\n"));

                        ns.Close();
                    }
                    catch
                    {
                        ns.Write(Encoding.ASCII.GetBytes("{\"status\":\"ERROR\"}\r\n"));
                        ns.Close();
                    }
                }
                else
                    Thread.Sleep(50);
            }
        }
    }
}