using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Unmute : BaseCommandModule {

        [Command("Unmute"), Description("Unmutes a Member")]
        public async Task UnmuteCommand(CommandContext ctx, [Description("The Member to unmute")] DiscordMember member) {
            await member.RevokeRoleAsync(ctx.Guild.GetRole(Program.MutedRole));
            var PS = await Players.SQLiteController.GetPlayer(member.Id);
            PS.Muted = false;
            await PS.Save();

            DiscordEmbedBuilder Unmute = new DiscordEmbedBuilder {
                Title = $"Unmute | {member.Username}",
                Description = $"**{member.Mention} has been unmuted**",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Unmute);
        }
    }
}