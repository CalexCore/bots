using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Nerva.Bots.Helpers
{
    public class Request
    {
        public static bool Api(string method, ISocketMessageChannel channel, out string resultString)
        {
            string[] apiLinks = new string[]
            {
                "xnv1.getnerva.org",
                "xnv2.getnerva.org"
            };

            resultString = null;

            for (int i = 0; i < apiLinks.Length; i++)
                if (Http($"https://{apiLinks[i]}/api/{method}.php", out resultString))
                    return true;

            channel.SendMessageAsync("Sorry... All API's are down. The zombie apocalyse is upon us! :scream:");
            return false;
        }

        public static bool Http(string url, out string returnString)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                using (Stream stream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    returnString = reader.ReadToEnd();
                }
                    
                return true;
            }
            catch (Exception ex)
            {
                Log.Write($"Error attempting HTTP request {url}");
                Log.WriteNonFatalException(ex);
                returnString = null;
                return false;
            }
        }
    }

    public class DiscordResponse
    {
        public static void Reply(SocketUserMessage msg, bool privateOnly = false, string text = null, Embed embed = null)
        {
            if (text == null)
                text = string.Empty;

            bool isDev = (msg.Author.Id == Globals.Bot.Config.OwnerID);

            //only angrywasp is allowed to post anywhere
            //todo: make a role that give people authority to issue bot commands from anywhere
            if (isDev && Globals.DevMode)
            {
                msg.Channel.SendMessageAsync(text, false, embed);
                return;
            }

            //we only show the message if the channel is the bot channel
            //privateOnly allows a per message override to force the reply to a DM
            //otherwise we send the user a dm and delete the message
            if (msg.Channel.Id == Globals.Bot.Config.BotChannelID && !privateOnly)
                msg.Channel.SendMessageAsync(text, false, embed);
            else
            {
                Discord.UserExtensions.SendMessageAsync(msg.Author, text, false, embed);
                if (msg.Channel.GetType() != typeof(SocketDMChannel))
                    msg.DeleteAsync();
            }
        }
    }
}