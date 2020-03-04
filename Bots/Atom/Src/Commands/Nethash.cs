using System;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
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
            var em = new EmbedBuilder()
            .WithAuthor("Amity Network Stats", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("Network hashrate")
            .WithColor(Color.DarkRed)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            Request.ApiAll(AtomBotConfig.GetSeedNodes(), "daemon/get_info", msg.Channel, (rd) =>
            {
                foreach (var r in rd)
                {
                    string result = "No response...";

                    if (r.Value != null)
                    {
                        result = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(r.Value.ResultString).Result.Difficulty.ToString();

                        float hr = JsonConvert.DeserializeObject<JsonResult<NodeInfo>>(r.Value.ResultString).Result.Difficulty / 60.0f;
                        result = $"{hr} h/s";

                        float kh = (float)Math.Round(hr / 1000.0f, 2);
                        float mh = (float)Math.Round(hr / 1000000.0f, 2);
                        if (mh > 1)
                            result = $"{mh} mh/s";
                        else
                            result = $"{kh} kh/s";
                    }

                    em.AddField(r.Key, result);
                }

                DiscordResponse.Reply(msg, embed: em.Build());
            });
        }
    }
}