using System;
using System.Collections.Generic;    
using System.Linq;    
using System.Text;    
using System.Net.Sockets;    
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using AngryWasp.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MagellanServer
{
    public class RpcListener
    {
        public void Start()
        { 
            Thread rpcThread = new Thread(new ThreadStart(InitRpcLoop));
            rpcThread.Start();  
        }

        private void InitRpcLoop()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 12345);    
            listener.Start();     
            Log.Instance.Write("Local endpoint: " + listener.LocalEndpoint);    
            Log.Instance.Write("RPC server initialized");

            while(true)
            {
                if (listener.Pending())
                {
                    Task.Run(() =>
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

                            if (json.method == null)
                                Log.Instance.Write(Log_Severity.Warning, "No JSON method provided");
                            else
                            {
                                Log.Instance.Write($"Received new request: {json.method}");
                                switch (json.method.Value)
                                {
                                    case "submit":
                                    {   
                                        NodeMapEntry nme = JsonConvert.DeserializeObject<RpcRequest<NodeMapEntry>>(content).Params;
                                        NodeMapDataStore.Add(nme);
                                        ok = true;
                                    }
                                    break;
                                    case "fetch":
                                    {
                                        ok = true;
                                    }
                                    break;
                                    default:
                                        Log.Instance.Write($"Invalid request: {json.Method}");
                                    break;
                                }
                            }

                            if (ok)
                                ns.Write(Encoding.ASCII.GetBytes("{\"status\":\"OK\"}\r\n"));
                            else
                                ns.Write(Encoding.ASCII.GetBytes("{\"status\":\"ERROR\"}\r\n"));

                            ns.Close();
                        }
                        catch
                        {
                            ns.Write(Encoding.ASCII.GetBytes("{\"status\":\"ERROR\"}\r\n"));
                            ns.Close();
                        }
                        
                    });
                }
                else
                    Thread.Sleep(50);
            }
        }
    }
}