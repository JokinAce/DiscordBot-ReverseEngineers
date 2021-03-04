using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using static DiscordBotEthan.Players.SQLiteController;
using static DiscordBotEthan.Program;
using System.Data.SQLite;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace DiscordBotEthan.Commands {

    public class Remindme : BaseCommandModule {

        [Command("Remindme"), Cooldown(1, 20, CooldownBucketType.User), Description("Remind yourself something in the future")]
        public async Task RemindmeCommand(CommandContext ctx, [Description("When to remind you (d/h/m/s) Ex. 7d for 7 Days")] string When, [Description("What to remind you to"), RemainingText] string What = "No reminder message specified") {
            double Time = JokinsCommon.Methods.TimeConverter(When);
            DateTime dateTime = DateTime.Now.AddMilliseconds(Time);

            var SQLC = new Players.SQLiteController();
            var output = await SQLC.GetReminderWithID((long)ctx.Member.Id);

            if (output.Count() == 1) {
                await ctx.RespondAsync("You already have a Reminder running");
                return;
            }

            DiscordEmbedBuilder Reminder = new DiscordEmbedBuilder {
                Title = $"Reminder | {ctx.Member.Username}",
                Description = $"**Ok, I will remind you the following on {dateTime:dd.MM.yyyy HH:mm}:**\n{What}",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Reminder);

            await SQLC.AddReminder((long)ctx.Member.Id, (long)ctx.Channel.Id, dateTime.ToBinary(), What);

            _ = Task.Run(async () => {
                await Task.Delay((int)Time);

                await ctx.RespondAsync($":alarm_clock:, {ctx.Member.Mention} you wanted me to remind you the following:\n\n{What}");
                await SQLC.DeleteRemindersWithDate(dateTime.ToBinary());
            });
        }
    }
}