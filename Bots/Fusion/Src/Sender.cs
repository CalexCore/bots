using System;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Discord;
using Discord.WebSocket;
using Nerva.Bots.Helpers;

namespace Fusion
{
    public static class Sender
    {
        public static bool IsPrivateMessage(SocketMessage msg) => (msg.Channel.GetType() == typeof(SocketDMChannel));

        public static async Task PublicReply(SocketUserMessage userMsg, string text, Embed embed = null) => await Reply(userMsg, true, text, embed);

        public static async Task PrivateReply(SocketUserMessage userMsg, string text, Embed embed = null) => await Reply(userMsg, false, text, embed);

        public static async Task SendPrivateMessage(SocketUser user, string text, Embed embed = null)
        {
            try
            {
                await Discord.UserExtensions.SendMessageAsync(user, text, false, embed);
            }
            catch (Exception)
            {
                await Nerva.Bots.Helpers.Log.Write(Log_Severity.Warning, $"Sending message to {user.Username} failed");
            }
        }

        public static async Task Reply(SocketUserMessage userMsg, bool allowPublic, string text, Embed embed = null)
        {
            try
            {
                if (text == null)
                    text = string.Empty;

                if (allowPublic)
                    await userMsg.Channel.SendMessageAsync(text, false, embed);
                else
                {
                    await Discord.UserExtensions.SendMessageAsync(userMsg.Author, text, false, embed);
                    if (!IsPrivateMessage(userMsg))
                        await userMsg.DeleteAsync();
                }
            }
            catch (Exception)
            {
                await Nerva.Bots.Helpers.Log.Write(Log_Severity.Warning, $"Sending message to {userMsg.Author.Username} failed");
            }
            
        }
    }
}