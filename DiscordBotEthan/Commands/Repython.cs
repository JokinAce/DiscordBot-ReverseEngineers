using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Repython : BaseCommandModule {

        [Command("Repython"), RequireRoles(RoleCheckMode.Any, "coder", "C# Global Elite"),Hidden]
        public async Task RepythonCommand(CommandContext ctx, [RemainingText] string code) {
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

                proc.Start();
                proc.WaitForExit();
                var result = await proc.StandardOutput.ReadToEndAsync().ConfigureAwait(false);

                if (result != null && !string.IsNullOrWhiteSpace(result)) {
                    await ctx.RespondAsync("Result:\n" + result).ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("No error but no return either").ConfigureAwait(false);
                }
            } catch (Exception ex) {
                await ctx.RespondAsync("You fucked up\n" + string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message)).ConfigureAwait(false);
            }
        }
    }
}