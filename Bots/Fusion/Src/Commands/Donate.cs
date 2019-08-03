using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;

namespace Fusion.Commands
{
    [Command("donate", "Get information on where to send your donations")]
    public class Donate : ICommand
    {
        public async Task Process(SocketUserMessage msg)
        {
            var em = new EmbedBuilder()
            .WithAuthor("Make A Donation", Globals.Client.CurrentUser.GetAvatarUrl())
            .WithDescription("Would you like to make a donation?\n" +
            "If you would like to remain anonymous, please omit the payment id")
            .WithColor(Color.Gold)
            .WithThumbnailUrl(Globals.Client.CurrentUser.GetAvatarUrl());

            foreach (var j in ((FusionBotConfig)(Globals.Bot.Config)).AccountJson.Accounts)
                if (j.Display)
                    em.AddField($"__{j.Name}__", $"{j.Address}\n\u200b");

            em.AddField("Payment ID", $"{IdEncrypter.Encrypt(msg.Author.Id, 0)}\n\u200b");

            await DiscordResponse.Reply(msg, privateOnly: true, embed: em.Build());
        }
    }
}