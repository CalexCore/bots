using Discord.WebSocket;
using Nerva.Bots.Plugin;

namespace Fusion.Commands.Gaming
{
    [Command("mynumbers", "Shows what numbers you have bought in the lottery")]
    public class MyNumbers : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            Sender.PublicReply(msg, LotteryManager.CurrentGame.GetUsersNumbers(msg.Author.Id));
        }
    }
}