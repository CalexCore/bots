using System;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Discord;
using Discord.WebSocket;

namespace Nerva.Bots.Helpers
{
    public class Request
    {
        public static bool Api(string[] apiLinks, string method, ISocketMessageChannel channel, out string resultString)
        {
            resultString = null;

            for (int i = 0; i < apiLinks.Length; i++)
                if (Http($"{apiLinks[i]}/api/{method}.php", out resultString))
                    return true;

            channel.SendMessageAsync("Sorry... All API's are down. The zombie apocalyse is upon us! :scream:");
            return false;
        }

        public static bool Http(string url, out string returnString)
        {
            string error = null;
            if (NetHelper.HttpRequest(url, out returnString, out error))
                return true;

            Log.Write(Log_Severity.Error, error);
            return false;
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
