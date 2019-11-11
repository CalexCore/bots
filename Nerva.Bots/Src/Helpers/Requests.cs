using System;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Discord;
using Discord.WebSocket;

namespace Nerva.Bots.Helpers
{
    public class RequestData
    {
        public string ResultString { get; set; }

        public string ErrorString { get; set; }
        public bool IsError => !string.IsNullOrEmpty(ErrorString);
    }

    public class Request
    {
        public static async Task<RequestData> Api(string[] apiLinks, string method, ISocketMessageChannel channel)
        {
            for (int i = 0; i < apiLinks.Length; i++)
            {
                RequestData rd = await Http($"{apiLinks[i]}/api/{method}/");

                if (!rd.IsError)
                    return rd;
            }

            await channel.SendMessageAsync("Sorry... All API's are down. The zombie apocalyse is upon us! :scream:");
            return null;
        }


        public static async Task<RequestData> Http(string url)
        {
            string rs = null, e = null;

            if (!NetHelper.HttpRequest(url, out rs, out e))
                await Log.Write(Log_Severity.Error, e);

            return new RequestData
            {
                ErrorString = e,
                ResultString = rs
            };
        }
    }

    public class DiscordResponse
    {
        public static async Task Reply(SocketUserMessage msg, bool privateOnly = false, string text = null, Embed embed = null)
        {
            try
            {
                if (text == null)
                    text = string.Empty;

                if (msg.Channel.GetType() != typeof(SocketDMChannel))
                {
                    bool isRole = false;
                    var userRoles = ((SocketGuildUser)msg.Author).Roles;
                    foreach(SocketRole role in userRoles)
                        if (Globals.Bot.Config.DevRoleIds.Contains(role.Id))
                        {
                            isRole = true;
                            break;
                        }
                    
                    if (isRole)
                    {
                        await msg.Channel.SendMessageAsync(text, false, embed);
                        return;
                    }

                    if (Globals.Bot.Config.BotChannelIds.Contains(msg.Channel.Id) && !privateOnly)
                        await msg.Channel.SendMessageAsync(text, false, embed);
                    else
                    {
                        await Discord.UserExtensions.SendMessageAsync(msg.Author, text, false, embed);
                        await msg.DeleteAsync();
                    }
                }
                else
                {
                    await Discord.UserExtensions.SendMessageAsync(msg.Author, text, false, embed);
                }
            }
            catch (Exception)
            {
                await Log.Write($"Count not send reply to {msg.Author.Username}");
            }
        }
    }
}
