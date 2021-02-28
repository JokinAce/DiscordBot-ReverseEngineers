using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Remindme : BaseCommandModule {

        [Command("Remindme"), Description("Remind yourself something in the future")]
        public async Task RemindmeCommand(CommandContext ctx, [Description("When to remind you (d/h/m/s) Ex. 7d for 7 Days")] string When, [Description("What to remind you to"), RemainingText] string What = "No reminder message specified") {
            double Time = JokinsCommon.Methods.TimeConverter(When);

            DiscordEmbedBuilder Reminder = new DiscordEmbedBuilder {
                Title = $"Reminder | {ctx.Member.Username}",
                Description = $"**Ok, I will remind you the following on {DateTime.Now.AddMilliseconds(Time):dd.MM.yyyy HH:mm}:**\n{What}",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Reminder);


            _ = Task.Run(async () => {
                await Task.Delay((int)Time);
                await ctx.RespondAsync($":alarm_clock:, {ctx.Member.Mention} you wanted me to remind you the following:\n\n{What}");
            });
        }
    }
}