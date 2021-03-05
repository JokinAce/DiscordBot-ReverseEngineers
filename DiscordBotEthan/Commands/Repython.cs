using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace DiscordBotEthan.Commands {

    public class Repython : BaseCommandModule {

        [Command("Repython"), RequireRoles(RoleCheckMode.Any, "coder", "C# Global Elite"), Hidden]
        public async Task RepythonCommand(CommandContext ctx, [RemainingText] string code) {
            if (Program.BlacklistedMembers.All(x => x == ctx.Member.Id)) {
                return;
            }

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1) {
                await ctx.RespondAsync("You need to wrap the code into a code block.");
                return;
            }

            var cs = code[cs1..cs2].Replace("\"", "'");
            await ctx.RespondAsync("Beginning execution");

            try {
                var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "python",
                        Arguments = $"-c \"{cs}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                if (!proc.Start() || !proc.WaitForExit(5000)) {
                    proc.Kill();
                    var tempmsg = await ctx.RespondAsync("Timeout");
                    var Jokin = await ctx.Guild.GetMemberAsync(447781010315149333);
                    await Jokin.SendMessageAsync("Timeout on repython command. Check " + tempmsg.JumpLink);
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