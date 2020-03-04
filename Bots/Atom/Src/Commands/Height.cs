using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

#pragma warning disable 4014

namespace Atom.Commands
{
    [Command("height", "Get the current chain height")]
    public class Height : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            var em = new EmbedBuilder()
            .WithAuthor("Amity Network Stats", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("Network height")
            .WithColor(Color.DarkRed)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            Request.ApiAll(AtomBotConfig.GetSeedNodes(), "daemon/get_block_count", msg.Channel, (rd) =>
            {
                foreach (var r in rd)
                {
                    string result = "No response...";

                    if (r.Value != null)
                        result = JsonConvert.DeserializeObject<JsonResult<GetBlockCount>>(r.Value.ResultString).Result.Count.ToString();

                    em.AddField(r.Key, result);
                }

                DiscordResponse.Reply(msg, embed: em.Build());
            });
        }
    }
}
