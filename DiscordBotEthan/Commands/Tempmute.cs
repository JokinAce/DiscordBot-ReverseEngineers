using Dapper;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Tempmute : BaseCommandModule {

        [Command("Tempmute"), RequirePermissions(DSharpPlus.Permissions.Administrator), Description("Temporarily mutes the User")]
        public async Task TempmuteCommand(CommandContext ctx, [Description("The Member to mute (ID, Mention, Username)")] DiscordMember member, [RemainingText, Description("Length (d/h/m/s) Ex. 7d for 7 Days")] string time) {
            double Time = JokinsCommon.Methods.TimeConverter(time);
            DateTime dateTime = DateTime.Now.AddMilliseconds(Time);
            DiscordRole MutedRole = ctx.Guild.GetRole(Program.MutedRole);

            var SQLC = new Players.SQLiteController();
            var PS = await SQLC.GetPlayer(member.Id);
            var output = await SQLC.GetTempmuteWithID((long)ctx.Member.Id);

            if (output != null) {
                await ctx.RespondAsync("That Member is already muted");
                return;
            }

            PS.Muted = true;
            await PS.Save();
            await member.GrantRoleAsync(MutedRole);
            await SQLC.AddTempmute((long)ctx.Member.Id, dateTime.ToBinary());

            DiscordEmbedBuilder TempMute = new DiscordEmbedBuilder {
                Title = $"TempMute | {member.Username}",
                Description = $"**{member.Mention} has been muted for {time}\nUnmuted on {dateTime:dd.MM.yyyy HH:mm}**",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: TempMute);

            _ = Task.Run(async () => {
                try {
                    await Task.Delay((int)Time);

                    var PS = await SQLC.GetPlayer(member.Id);
                    PS.Muted = false;

                    await PS.Save();
                    await SQLC.DeleteTempmutesWithID((long)member.Id);
                    await member.RevokeRoleAsync(MutedRole);
                } catch (Exception) {
                    ctx.Client.Logger.LogInformation($"Failed the Tempmute process for {ctx.Member.Username + ctx.Member.Discriminator}");
                }
            });
        }
    }
}