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

    [Group("Remindme"), Aliases("Reminder", "Remind")]
    public class Remindme : BaseCommandModule {

        [GroupCommand, Cooldown(1, 20, CooldownBucketType.User), Description("Remind yourself something in the future")]
        public async Task RemindmeCommand(CommandContext ctx, [Description("When to remind you (d/h/m/s) Ex. 7d for 7 Days")] string When, [Description("What to remind you to"), RemainingText] string What = "No reminder message specified") {
            double Time = JokinsCommon.Methods.TimeConverter(When);
            DateTime dateTime = DateTime.Now.AddMilliseconds(Time);

            var SQLC = new Players.SQLiteController();
            var output = await SQLC.GetRemindersWithID((long)ctx.Member.Id);

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

        [Command("clear"), Cooldown(1, 20, CooldownBucketType.User), Description("Clear your Reminders")]
        public async Task ClearCommand(CommandContext ctx) {
            var SQLC = new Players.SQLiteController();
            await SQLC.DeleteRemindersWithID((long)ctx.Member.Id);

            DiscordEmbedBuilder Reminder = new DiscordEmbedBuilder {
                Title = $"Reminder | {ctx.Member.Username}",
                Description = $"**Ok, I cleared your Reminders**",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Reminder);
        }

        [Command("add"), Description("Remind someone, something in the future"), RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task AddCommand(CommandContext ctx, DiscordMember member, [Description("When to remind someone (d/h/m/s) Ex. 7d for 7 Days")] string When, [Description("What to remind someone to"), RemainingText] string What = "No reminder message specified") {
            double Time = JokinsCommon.Methods.TimeConverter(When);
            DateTime dateTime = DateTime.Now.AddMilliseconds(Time);

            var SQLC = new Players.SQLiteController();
            var output = await SQLC.GetRemindersWithID((long)member.Id);

            if (output.Count() == 1) {
                await ctx.RespondAsync("He already has a Reminder running");
                return;
            }

            DiscordEmbedBuilder Reminder = new DiscordEmbedBuilder {
                Title = $"Reminder | {member.Username}",
                Description = $"**Ok, I will remind him the following on {dateTime:dd.MM.yyyy HH:mm}:**\n{What}",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Reminder);

            await SQLC.AddReminder((long)member.Id, (long)ctx.Channel.Id, dateTime.ToBinary(), What);

            _ = Task.Run(async () => {
                await Task.Delay((int)Time);

                await ctx.RespondAsync($":alarm_clock:, {member.Mention} someone wanted me to remind you the following:\n\n{What}");
                await SQLC.DeleteRemindersWithDate(dateTime.ToBinary());
            });
        }
    }
}