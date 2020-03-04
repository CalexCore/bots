using System.Text;
using System.Net;
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
        private const string RESULT_OK = "{\"status\":\"OK\"}\r\n";
        private const string RESULT_BAD = "{\"status\":\"ERROR\"}\r\n";

        private NodeMapDataStore dataStore = new NodeMapDataStore();

        public NodeMapDataStore DataStore => dataStore;

        public delegate void MapDataChangedEventHandler();

        public event MapDataChangedEventHandler MapDataChanged;

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
                    Task.Run( () => {
                        HandleRequest(context);
                    });
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

            Log.Instance.Write($"Received new request: {json.method}");
            switch (json.method.Value)
            {
                case "submit":
                    {
                        SubmitParams sp = JsonConvert.DeserializeObject<RpcRequest<SubmitParams>>(text).Params;
                        bool ne = false;
                        bool ok = dataStore.Add(sp, out ne);
                        if (ok)
                        {
                            if (ne)
                                MapDataChanged?.Invoke();
                        }

                        string result = ok ? RESULT_OK : RESULT_BAD;
                        response.OutputStream.Write(Encoding.ASCII.GetBytes(result));
                        context.Response.Close();
                    }
                    break;
                case "fetch":
                    {
                        FetchParams fp = JsonConvert.DeserializeObject<RpcRequest<FetchParams>>(text).Params;
                        response.OutputStream.Write(Encoding.ASCII.GetBytes(dataStore.Fetch(fp)));
                        context.Response.Close();
                    }
                    break;
                case "prune":
                    {
                        PruneParams pp = JsonConvert.DeserializeObject<RpcRequest<PruneParams>>(text).Params;

                        if (string.IsNullOrEmpty(pp.Key))
                        {
                            Log.Instance.Write(Log_Severity.Warning, "No access key provided");
                            return;
                        }

                        if (!Config.AllowedKeys.Contains(pp.Key))
                        {
                            Log.Instance.Write(Log_Severity.Warning, "Invalid access key");
                            return;
                        }

                        int pc = 0;
                        bool ok = dataStore.Prune(pp, out pc);
                        if (ok && !pp.DryRun)
                        {
                            if (pc > 0)
                                MapDataChanged?.Invoke();
                        }

                        string result = ok ? RESULT_OK : RESULT_BAD;
                        response.OutputStream.Write(Encoding.ASCII.GetBytes(result));
                        context.Response.Close();
                    }
                    break;
                default:
                    Log.Instance.Write($"Invalid request: {json.Method}");
                    break;
            }
        }
    }
}