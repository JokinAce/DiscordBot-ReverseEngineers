using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Repython : BaseCommandModule {
        private readonly string[] BlacklistedCode = { "os", "socket", "importlib", "sys", "subprocess", "asyncore", "hostname", "ping", "shutdown", "system" };

        [Command("Repython"), Cooldown(2, 10, CooldownBucketType.User), RequireRoles(RoleCheckMode.Any, "coder", "C# Global Elite"), Hidden]
        public async Task RepythonCommand(CommandContext ctx, [RemainingText] string code) {
            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1) {
                await ctx.RespondAsync("You need to wrap the code into a code block.");
                return;
            }

            var cs = code[cs1..cs2].Replace("\"", "'");

            if (Program.BlacklistedMembers.Any(x => x == ctx.Member.Id)) {
                await ctx.RespondAsync("You are blacklisted");
                return;
            } else if (BlacklistedCode.Any(x => cs.Contains(x))) {
                await ctx.RespondAsync($"Something doesn't seem right.. <@{Program.BotOwner}> verify this");

                var msg = await ctx.Channel.GetNextMessageAsync(x => x.Author.Id == Program.BotOwner);
                if (msg.Result.Content.ToLower() != "verified" || msg.TimedOut) {
                    await ctx.RespondAsync("Bot Owner didn't verify of execution");
                    return;
                }
            }

            await ctx.RespondAsync("Beginning execution");

            try {
                var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "python3",
                        Arguments = $"-c \"{cs}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                if (!proc.Start() || !proc.WaitForExit(5000)) {
                    proc.Kill();
                    var tempmsg = await ctx.RespondAsync("Timeout");
                    var Jokin = await ctx.Guild.GetMemberAsync(Program.BotOwner);
                    await Jokin.SendMessageAsync("Timeout on repython command. Check\n" + tempmsg.JumpLink);
                    return;
                }

                var result = await proc.StandardOutput.ReadToEndAsync().ConfigureAwait(false);

                if (result != null && !string.IsNullOrWhiteSpace(result)) {
                    DiscordEmbedBuilder exec = new DiscordEmbedBuilder {
                        Title = $"Execution | Result",
                        Description = result,
                        Color = Program.EmbedColor,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                        Timestamp = DateTimeOffset.Now
                    };
                    await ctx.RespondAsync(embed: exec).ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("No C# error but no result either").ConfigureAwait(false);
                }
            } catch (Exception ex) {
                await ctx.RespondAsync("You fucked up\n" + string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message)).ConfigureAwait(false);
            }
        }
    }
}