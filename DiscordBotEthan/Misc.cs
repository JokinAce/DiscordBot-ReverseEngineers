using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static DiscordBotEthan.Program;

namespace DiscordBotEthan {

    public static class Misc {

        public static async Task Warn(DiscordChannel channel, DiscordUser member, string reason) {
            var WarnS = await Players.SQLiteController.GetPlayer(member.Id);

            bool LocalMute = false;

            if ((WarnS.Warns.Count + 1) >= 3) {
                _ = Task.Run(async () => {
                    try {
                        DiscordRole muterole = channel.Guild.GetRole(MutedRole);
                        var CMember = await channel.Guild.GetMemberAsync(member.Id);
                        await CMember.GrantRoleAsync(muterole);
                        await CMember.SendMessageAsync("You got muted for 24 Hours because you have equal or more then 3 Warns.");
                        await Task.Delay(86400000);
                        var WarnS = await Players.SQLiteController.GetPlayer(member.Id);
                        WarnS.Muted = false;
                        await WarnS.Save();
                        await CMember.RevokeRoleAsync(muterole);
                    } catch (Exception) {
                        discord.Logger.LogInformation($"Failed the Warn Tempmute process for {member.Mention}");
                    }
                });
                WarnS.Muted = true;
                LocalMute = true;
            }

            DiscordEmbedBuilder Warns = new DiscordEmbedBuilder {
                Title = $"Warns | {member.Username}",
                Description = $"**{member.Mention} has been warned for the following Reason:**\n{reason}\n**Muted: {(LocalMute ? $"True\nUnmuted on {DateTime.Now.AddMilliseconds(86400000):dd.MM.yyyy HH:mm}" : "False")}**",
                Color = EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            var msg = await channel.SendMessageAsync(embed: Warns);

            WarnS.Warns.Add($"{reason} | [Event]({msg.JumpLink})");
            await WarnS.Save();
        }
    }
}