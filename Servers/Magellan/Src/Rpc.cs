using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using AngryWasp.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AngryWasp.Serializer;
using System.Threading.Tasks;
using System.IO;

namespace MagellanServer
{
    public class RpcListener
    {
        private NodeMapDataStore dataStore = new NodeMapDataStore();

        public void Start(NodeMapDataStore ds, int port)
        {
            dataStore = ds;
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{port}/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();
            listener.Start();
            Log.Instance.Write($"Local RPC endpoint on port {port}");
            Log.Instance.Write("RPC server initialized");

            Task.Run(() =>
            {
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HandleRequest(context);
                }
            });
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            response.ContentType = "application/json";

            string text;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            string method = request.Url.Segments[1];

            dynamic json = JObject.Parse(text);
            bool ok = false;
            string resultData = null;

            Log.Instance.Write($"Received new request: {json.method}");
            switch (json.method.Value)
            {
                case "submit":
                    {
                        SubmitParams sp = JsonConvert.DeserializeObject<RpcRequest<SubmitParams>>(text).Params;
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
                        FetchParams fp = JsonConvert.DeserializeObject<RpcRequest<FetchParams>>(text).Params;
                        resultData = dataStore.Fetch(fp);
                        ok = true;
                    }
                    break;
                case "prune":
                    {
                        PruneParams pp = JsonConvert.DeserializeObject<RpcRequest<PruneParams>>(text).Params;

                        if (string.IsNullOrEmpty(pp.Key))
                        {
                            Log.Instance.Write(Log_Severity.Warning, "No access key provided");
                            ok = false;
                            break;
                        }

                        if (!Config.AllowedKeys.Contains(pp.Key))
                        {
                            Log.Instance.Write(Log_Severity.Warning, "Invalid access key");
                            ok = false;
                            break;
                        }

                        
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

            string result = string.Empty;
            if (ok)
            {
                if (resultData == null)
                    result = "{\"status\":\"OK\"}\r\n";
                else
                    result = $"{{\"status\":\"OK\",\"result\":{resultData}}}\r\n";
            }
            else
                result = "{\"status\":\"ERROR\"}\r\n";

            response.OutputStream.Write(Encoding.ASCII.GetBytes(result));
            context.Response.Close();
        }
    }
}