using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("nethash", "Get the current network hashrate")]
    public class Nethash : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            RequestData rd = await Request.Api(AtomBotConfig.SeedNodes, "daemon/get_info", msg.Channel);
            if (!rd.IsError)
            {
                float hr = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(rd.ResultString).Result.Difficulty / 60.0f;
                string formatted = $"{hr} h/s";

                float kh = (float)Math.Round(hr / 1000.0f, 2);
                float mh = (float)Math.Round(hr / 1000000.0f, 2);
                if (mh > 1)
                    formatted = $"{mh} mh/s";
                else
                    formatted = $"{kh} kh/s";

                await DiscordResponse.Reply(msg, text: $"Current nethash: {formatted}");
            }
        }
    }
}