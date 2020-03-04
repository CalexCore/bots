using System;
using System.Collections.Generic;
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
        public static RequestData ApiAny(List<string> apiLinks, string method, ISocketMessageChannel channel)
        {
            foreach (var apiLink in apiLinks)
            {
                RequestData rd = Http($"{apiLink}/api/{method}/");

                if (!rd.IsError)
                    return rd;
            }

            channel.SendMessageAsync("Sorry... All API's are down. The zombie apocalyse is upon us! :scream:");
            return null;
        }

        public static void ApiAll(List<string> apiLinks, string method, ISocketMessageChannel channel, Action<Dictionary<string, RequestData>> action)
        {
            Dictionary<string, RequestData> ret = new Dictionary<string, RequestData>();
            foreach (var apiLink in apiLinks)
            {
                if (ret.ContainsKey(apiLink))
                    continue;

                RequestData rd = Http($"http://{apiLink}/api/{method}/");
                ret.Add(apiLink, rd.IsError ? null : rd);
            }

            bool allNull = true;

            foreach (var r in ret.Values)
            {
                if (r != null)
                {
                    allNull = false;
                    break;
                }
            }

            if (allNull)
                channel.SendMessageAsync("Sorry... All API's are down. The zombie apocalyse is upon us! :scream:");
            else
                action(ret);
        }

        public static RequestData Http(string url)
        {
            string rs = null, e = null;

            if (!NetHelper.HttpRequest(url, out rs, out e))
                Log.Write(Log_Severity.Error, e);

            return new RequestData
            {
                ErrorString = e,
                ResultString = rs
            };
        }

        public static void Http(string url, Action<RequestData> action)
        {
            string rs = null, e = null;

            if (!NetHelper.HttpRequest(url, out rs, out e))
                Log.Write(Log_Severity.Error, e);

            action(new RequestData
            {
                ErrorString = e,
                ResultString = rs
            });
        }
    }

    public class DiscordResponse
    {
        public static void Reply(SocketUserMessage msg, bool privateOnly = false, string text = null, Embed embed = null)
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
                        msg.Channel.SendMessageAsync(text, false, embed);
                        return;
                    }

                    if (Globals.Bot.Config.BotChannelIds.Contains(msg.Channel.Id) && !privateOnly)
                        msg.Channel.SendMessageAsync(text, false, embed);
                    else
                    {
                        Discord.UserExtensions.SendMessageAsync(msg.Author, text, false, embed);
                        msg.DeleteAsync();
                    }
                }
                else
                {
                    Discord.UserExtensions.SendMessageAsync(msg.Author, text, false, embed);
                }
            }
            catch (Exception)
            {
                Log.Write($"Count not send reply to {msg.Author.Username}");
            }
        }
    }
}
