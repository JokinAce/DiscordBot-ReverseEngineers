using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Tempmute : BaseCommandModule {

        [Command("Tempmute"), RequirePermissions(DSharpPlus.Permissions.Administrator), Description("Temporarily mutes the User")]
        public async Task TempmuteCommand(CommandContext ctx, [Description("The Member to mute (ID, Mention, Username)")] DiscordMember member, [RemainingText, Description("Length (d/h/m/s) Ex. 7d for 7 Days")] string time) {
            var PS = await Program.PlayerSystem.GetPlayer(member.Id);
            PS.Muted = true;
            await PS.Save(member.Id);

            double Time = JokinsCommon.Methods.TimeConverter(time);

            DiscordEmbedBuilder TempMute = new DiscordEmbedBuilder {
                Title = $"TempMute | {member.Username}",
                Description = $"**{member.Mention} has been muted for {time}\nUnmuted on {DateTime.Now.AddMilliseconds(Time):dd.MM.yyyy HH:mm}**",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: TempMute);

            _ = Task.Run(async () => {
                try {
                    DiscordRole MutedRole = ctx.Guild.GetRole(Program.MutedRole);
                    await member.GrantRoleAsync(MutedRole);
                    await Task.Delay((int)Time);
                    var PS = await Program.PlayerSystem.GetPlayer(member.Id);
                    PS.Muted = false;
                    await PS.Save(member.Id);
                    await member.RevokeRoleAsync(MutedRole);
                } catch (Exception) {
                    ctx.Client.Logger.LogInformation($"Failed the Tempmute process for {member.Mention}");
                }
            });
        }
    }
}