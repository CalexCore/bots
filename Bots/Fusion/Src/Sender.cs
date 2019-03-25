using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Fusion
{
    public static class Sender
    {
        public static bool IsPrivateMessage(SocketMessage msg) => (msg.Channel.GetType() == typeof(SocketDMChannel));

        public static async Task PublicReply(SocketUserMessage userMsg, string text, Embed embed = null) => await Reply(userMsg, true, text, embed);

        public static async Task PrivateReply(SocketUserMessage userMsg, string text, Embed embed) => await Reply(userMsg, false, text, embed);

        public static async Task SendPrivateReply(SocketUser userMsg, string text, Embed embed = null) => await Discord.UserExtensions.SendMessageAsync(userMsg, text, false, embed);

        public static async Task Reply(SocketUserMessage userMsg, bool allowPublic, string text, Embed embed = null)
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
    }
}