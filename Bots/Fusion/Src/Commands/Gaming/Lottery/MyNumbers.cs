using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;

namespace Fusion.Commands.Gaming
{
    [Command("mynumbers", "Shows what numbers you have bought in the lottery")]
    public class MyNumbers : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            await Sender.PublicReply(msg, LotteryManager.CurrentGame.GetUsersNumbers(msg.Author.Id));
        }
    }
}