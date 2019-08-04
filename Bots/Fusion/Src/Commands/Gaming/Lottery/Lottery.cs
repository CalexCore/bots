using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Plugin;

namespace Fusion.Commands.Gaming
{
    [Command("lottery", "Get stats about the current lottery game")]
    public class LotteryStats : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            EmbedBuilder eb = new EmbedBuilder()
            .WithAuthor($"Lottery Stats", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("Winners are grinners!")
            .WithColor(Color.DarkOrange)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            eb.AddField("Cost", $"{LotteryManager.CurrentGame.Parameters.TicketCost}xnv");
            eb.AddField("Prize", $"{LotteryManager.CurrentGame.Parameters.WinnerCount}x {LotteryManager.CurrentGame.Parameters.MinorPrize}xnv");
            eb.AddField("Jackpot", $"{LotteryManager.CurrentGame.JackpotAmount}xnv");
            eb.AddField("Tickets Left", $"{LotteryManager.CurrentGame.GetRemainingTickets()} / {LotteryManager.CurrentGame.Parameters.TicketCount}");

            await Sender.PublicReply(msg, null, eb.Build());
        }
    }
}