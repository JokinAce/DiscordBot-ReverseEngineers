﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Warns : BaseCommandModule {

        [Command("Warns"), Description("You can clear, add or show Warnings on a Member"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task WarnssCommand(CommandContext ctx, [Description("Method to do (clear/show/add)")] string method, [Description("Member to perform on")] DiscordMember member, [RemainingText, Description("Reason for the warn if method is add")] string reason = "No reason specified") {
            switch (method.ToLower()) {
                case "clear": {
                        var WarnS = await Program.PlayerSystem.GetPlayer(member.Id);
                        WarnS.Warns.Clear();
                        WarnS.Save(member.Id);

                        DiscordEmbedBuilder Warns = new DiscordEmbedBuilder {
                            Title = $"Warns | {member.Username}",
                            Description = $"**Warnings have been cleared for:**\n{member.Mention}",
                            Color = Program.EmbedColor,
                            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                            Timestamp = DateTimeOffset.Now
                        };
                        await ctx.RespondAsync(embed: Warns);
                        break;
                    }

                case "show": {
                        var WarnS = await Program.PlayerSystem.GetPlayer(member.Id);

                        DiscordEmbedBuilder Warns = new DiscordEmbedBuilder {
                            Title = $"Warns | {member.Username}",
                            Description = WarnS.Warns.Count == 0 ? $"{member.Mention} **has no warnings**" : $"{member.Mention} **has following Warns:**\n" + string.Join("\n", WarnS.Warns.ToArray()),
                            Color = Program.EmbedColor,
                            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                            Timestamp = DateTimeOffset.Now
                        };
                        await ctx.RespondAsync(embed: Warns);
                        break;
                    }

                case "add":
                    await Misc.Warn(ctx.Channel, member, reason);
                    break;

                default:
                    throw new ArgumentNullException();
            }
        }
    }
}