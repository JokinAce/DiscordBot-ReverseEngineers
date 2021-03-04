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

            using IDbConnection cnn = new SQLiteConnection(Program.ConnString);
            var output = await cnn.QueryFirstOrDefaultAsync("SELECT * FROM Tempmutes WHERE ID=@id", new { id = ctx.Member.Id }).ConfigureAwait(false);

            if (output != null) {
                await ctx.RespondAsync("That Member is already muted");
                return;
            }

            DiscordEmbedBuilder TempMute = new DiscordEmbedBuilder {
                Title = $"TempMute | {member.Username}",
                Description = $"**{member.Mention} has been muted for {time}\nUnmuted on {dateTime:dd.MM.yyyy HH:mm}**",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: TempMute);

            await cnn.ExecuteAsync("INSERT INTO Tempmutes (ID, Date) VALUES (@id, @date)", new { id = ctx.Member.Id, date = dateTime.ToBinary() }).ConfigureAwait(false);

            var PS = await Players.SQLiteController.GetPlayer(member.Id);
            PS.Muted = true;
            await PS.Save();

            _ = Task.Run(async () => {
                try {
                    DiscordRole MutedRole = ctx.Guild.GetRole(Program.MutedRole);
                    await member.GrantRoleAsync(MutedRole);

                    await Task.Delay((int)Time);

                    var PS = await Players.SQLiteController.GetPlayer(member.Id);
                    PS.Muted = false;
                    await PS.Save();

                    using IDbConnection cnn = new SQLiteConnection(Program.ConnString);
                    await cnn.ExecuteAsync("DELETE FROM Tempmutes WHERE ID=@id", new { id = ctx.Member.Id }).ConfigureAwait(false);

                    await member.RevokeRoleAsync(MutedRole);
                } catch (Exception) {
                    ctx.Client.Logger.LogInformation($"Failed the Tempmute process for {ctx.Member.Username + ctx.Member.Discriminator}");
                }
            });
        }
    }
}