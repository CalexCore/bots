using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml.Linq;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;
using MagellanServer;
using Nerva.Rpc.Daemon;
using Newtonsoft.Json;

namespace Tools
{
    public class MainClass
    {
        private static string[] urls = new string[]
        {
            "https://xnvex.pubnodes.com/api/getpeerlist.php",
            "http://127.0.0.1:8080/api/getpeerlist.php",
            "https://nerva.syzygy.cc/api/getpeerlist.php",
            "https://xnv1.getnerva.org/api/getpeerlist.php",
            "https://xnv2.getnerva.org/api/getpeerlist.php",
            "https://xnv3.getnerva.org/api/getpeerlist.php",
            "https://xnv4.getnerva.org/api/getpeerlist.php",
        };

        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineParser cmd = CommandLineParser.Parse(args);
            Log.CreateInstance(true);
            Serializer.Initialize();

            if (cmd["ip-list"] == null || string.IsNullOrEmpty(cmd["ip-list"].Value))
            {
                Log.Instance.Write(Log_Severity.Fatal, "No IP list provided");
                Environment.Exit(0);
            }

            string list = cmd["ip-list"].Value;

            if (!File.Exists(list))
            {
                Log.Instance.Write(Log_Severity.Fatal, "IP list not found");
                Environment.Exit(0);
            }

            List<Tuple<uint, uint>> subnets = new List<Tuple<uint, uint>>();
            List<string> badList = new List<string>();
            string[] lines = File.ReadAllLines(list);

            uint total = 0;

            foreach (var l in lines)
            {
                string[] p = l.Split('.', '/');

                uint ip = (Convert.ToUInt32(p[0]) << 24) |
                    (Convert.ToUInt32(p[1]) << 16) |
                    (Convert.ToUInt32(p[2]) << 8) |
                    Convert.ToUInt32(p[3]);

                int bits = Convert.ToInt32(p[4]);
                uint mask = 0xFFFFFFFF << (32 - bits);

                uint start = ip & mask;
                uint end = ip | ~mask;

                subnets.Add(new Tuple<uint, uint>(start, end));
                Log.Instance.Write($"Adding IP range: {ToIP(start)} - {ToIP(end)}");
                total += end - start;
            }

            Log.Instance.Write($"{total} IP's total");

            foreach (var url in urls)
            {
                string returnString, errorString;
                if (!NetHelper.HttpRequest(url, out returnString, out errorString))
                {
                    Log.Instance.Write(Log_Severity.Error, errorString);
                    Log.Instance.Write(Log_Severity.Error, $"Failed to load url: {url}");
                    continue;
                }

                Log.Instance.Write($"Got url: {url}");

                GetPeerListResponseData peerList = JsonConvert.DeserializeObject<GetPeerListResponseData>(returnString);
                RunList(peerList.GrayList, subnets, badList);
                RunList(peerList.WhiteList, subnets, badList);
            }

            File.WriteAllLines("GoogleList.txt", badList.ToArray());
            Log.Instance.Write($"DONE!");
        }

        static string ToIP(uint ip)
        {
            return String.Format("{0}.{1}.{2}.{3}", ip >> 24, (ip >> 16) & 0xff, (ip >> 8) & 0xff, ip & 0xff);
        }

        private static void RunList(List<GetPeerListResponseDataItem> list, List<Tuple<uint, uint>> subnets, List<string> badList)
        {
            List<string> pp = new List<string>();
            foreach (var p in list)
            {
                string ip = ToIP(p.IP);
                if (p.IP == 0)
                    continue; //skip 0 IP

                if (badList.Contains(ip))
                    continue; //already logged

                foreach (var s in subnets)
                {
                    if (p.IP >= s.Item1 && p.IP <= s.Item2)
                    {
                        Log.Instance.Write($"Match found: {ip}");
                        badList.Add(ip);
                    }
                }
            }
        }
    }
}