using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Resharp : BaseCommandModule {

        [Command("Resharp"), RequireOwner, Hidden] // Stole from https://github.com/Naamloos/ModCore/blob/master/ModCore/Commands/Eval.cs but I know now to use Microsoft.CodeAnalysis in the future if I need something like this again
        public async Task ResharpCommand(CommandContext ctx, [RemainingText] string code) {
            var msg = ctx.Message;

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1) {
                await ctx.RespondAsync("You need to wrap the code into a code block.");
                return;
            }

            var cs = code[cs1..cs2];
            await ctx.RespondAsync("Beginning execution");

            try {
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx);

                var sopts = ScriptOptions.Default.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext");
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                var script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                var result = await script.RunAsync(globals).ConfigureAwait(false);

                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await ctx.RespondAsync("Returned: \n" + result.ReturnValue.ToString()).ConfigureAwait(false);
                else
                    await ctx.RespondAsync("No error but no return either").ConfigureAwait(false);
            } catch (Exception ex) {
                await ctx.RespondAsync("You fucked up\n" + string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message)).ConfigureAwait(false);
            }
        }

        public class TestVariables {
            public DiscordMessage Message { get; set; }
            public DiscordChannel Channel { get; set; }
            public DiscordGuild Guild { get; set; }
            public DiscordUser User { get; set; }
            public DiscordMember Member { get; set; }
            public CommandContext Context { get; set; }

            public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx) {
                this.Client = client;

                this.Message = msg;
                this.Channel = msg.Channel;
                this.Guild = this.Channel.Guild;
                this.User = this.Message.Author;
                if (this.Guild != null)
                    this.Member = this.Guild.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                this.Context = ctx;
            }

            public DiscordClient Client;
        }
    }
}