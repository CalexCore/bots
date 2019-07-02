using System;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("nethash", "Get the current network hashrate")]
    public class Nethash : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            string result;
            if (Request.Api(AtomBotConfig.SeedNodes, "getinfo", msg.Channel, out result))
            {
                float hr = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(result).Result.Difficulty / 60.0f;
                string formatted = $"{hr} h/s";

                float kh = (float)Math.Round(hr / 1000.0f, 2);
                float mh = (float)Math.Round(hr / 1000000.0f, 2);
                if (mh > 1)
                    formatted = $"{mh} mh/s";
                else
                    formatted = $"{kh} kh/s";

                DiscordResponse.Reply(msg, text: $"Current nethash: {formatted}");
            }
        }
    }
}
