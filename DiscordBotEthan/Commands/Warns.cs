using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    [Group("Warns"), Aliases("Warn", "Warnings")]
    public class Warns : BaseCommandModule {

        [GroupCommand, Description("Shows all warns for said Member")]
        public async Task WarnsShowCommand(CommandContext ctx, [Description("The Member as Mention or ID/Username")] DiscordMember member) {
            var WarnS = await Program.PlayerSystem.GetPlayer(member.Id);

            DiscordEmbedBuilder Warns = new DiscordEmbedBuilder {
                Title = $"Warns | {member.Username}",
                Description = WarnS.Warns.Count == 0 ? $"{member.Mention} **has no warnings**" : $"{member.Mention} **has following Warns:**\n" + string.Join("\n", WarnS.Warns.ToArray()),
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Warns);
        }

        [Command("clear"), Description("Clears all warns for said Member"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task WarnsClearCommand(CommandContext ctx, [Description("The Member as Mention or ID/Username")] DiscordMember member) {
            var WarnS = await Program.PlayerSystem.GetPlayer(member.Id);
            WarnS.Warns.Clear();
            await WarnS.Save(member.Id);

            DiscordEmbedBuilder Warns = new DiscordEmbedBuilder {
                Title = $"Warns | {member.Username}",
                Description = $"**Warnings have been cleared for:**\n{member.Mention}",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Warns);
        }

        [Command("add"), Description("Adds a warn for said Member with a reason"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task WarnsAddCommand(CommandContext ctx, [Description("The Member as Mention or ID/Username")] DiscordMember member, [RemainingText, Description("Reason for the warn")] string reason = "No reason specified") => await Misc.Warn(ctx.Channel, member, reason);
    }
}