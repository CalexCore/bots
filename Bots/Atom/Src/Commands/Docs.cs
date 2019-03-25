using System;
using System.IO;
using Atom;
using Discord;
using Discord.WebSocket;
using Nerva.Bots;
using Nerva.Bots.Helpers;
using Nerva.Bots.Plugin;
using Newtonsoft.Json;

namespace Atom.Commands
{
    [Command("docs", "Get some useful docs")]
    public class Docs : ICommand
    {
        public void Process(SocketUserMessage msg)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Help.md");

            if (!File.Exists(path))
            {
                DiscordResponse.Reply(msg, text: $"Call the dev. No Help @ {path}");
            }
            else
            {
                string[] result = File.ReadAllLines(path);
                if (result.Length > 0)
                {
                    var em = new EmbedBuilder()
                    .WithAuthor("NERVA Documentation", Globals.Client.CurrentUser.GetAvatarUrl())
                    .WithDescription("The latest NERVA docs direct from BitBucket")
                    .WithColor(Color.DarkOrange)
                    .WithThumbnailUrl("https://getnerva.org/content/images/bitbucket-logo.png");

                    string fieldHeading = string.Empty;
                    string fieldData = string.Empty;
                    foreach (string line in result)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            em.AddField(fieldHeading, fieldData);
                            continue;
                        }

                        if (line.StartsWith("##"))
                        {
                            fieldHeading = $"__{line.Substring(2).Trim()}__";
                            fieldData = string.Empty;
                        }
                        else
                        {
                            fieldData += $"{line}\n";
                        }
                    }

                    DiscordResponse.Reply(msg, embed: em.Build());
                }
                else
                    DiscordResponse.Reply(msg, text: "Nope... Can't find the manual");
            }
        }
    }
}