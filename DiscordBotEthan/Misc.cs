using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static DiscordBotEthan.Program;

namespace DiscordBotEthan {

    public static class Misc {

        public static async Task Warn(DiscordChannel channel, DiscordUser member, string reason) {
            var SQLC = new Players.SQLiteController();
            var WarnS = await SQLC.GetPlayer(member.Id);

            bool LocalMute = false;

            if ((WarnS.Warns.Count + 1) >= 3) {
                if (!WarnS.Muted) {
                    await SQLC.AddTempmute((long)member.Id, DateTime.Now.AddMilliseconds(86400000).ToBinary());
                    _ = Task.Run(async () => {
                        try {
                            var Guild = await discord.GetGuildAsync(GuildID);
                            var MutedRole = Guild.GetRole(Program.MutedRole);
                            var CMember = await Guild.GetMemberAsync(member.Id);
                            await CMember.GrantRoleAsync(MutedRole);

                            await Task.Delay(86400000);

                            var PS = await SQLC.GetPlayer(member.Id);
                            PS.Muted = false;
                            await PS.Save();

                            await SQLC.DeleteTempmutesWithID((long)member.Id);
                            await CMember.RevokeRoleAsync(MutedRole);
                        } catch (Exception) {
                            discord.Logger.LogInformation($"Failed the Tempmute process for {member.Username + "#" + member.Discriminator}");
                        }
                    });
                }
                LocalMute = true;
                WarnS.Muted = true;
            }

            DiscordEmbedBuilder Warns = new DiscordEmbedBuilder {
                Title = $"Warns | {member.Username}",
                Description = $"**{member.Mention} has been warned for the following Reason:**\n{reason}\n**Muted: {(LocalMute ? $"True\nUnmuted on {DateTime.Now.AddMilliseconds(86400000):dd.MM.yyyy HH:mm}" : "False")}**",
                Color = EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            var msg = await channel.SendMessageAsync(Warns);

            WarnS.Warns.Add($"{reason} | [Event]({msg.JumpLink})");
            await WarnS.Save();
        }
    }
}