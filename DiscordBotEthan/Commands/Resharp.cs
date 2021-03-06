﻿using DSharpPlus;
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
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx, new Players.SQLiteController());

                var sopts = ScriptOptions.Default.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext");
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                var script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                var result = await script.RunAsync(globals).ConfigureAwait(false);

                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString())) {
                    DiscordEmbedBuilder exec = new DiscordEmbedBuilder {
                        Title = $"Execution | Returned",
                        Description = result.ReturnValue.ToString(),
                        Color = Program.EmbedColor,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                        Timestamp = DateTimeOffset.Now
                    };
                    await ctx.RespondAsync(embed: exec).ConfigureAwait(false);
                } else
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

            public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx, Players.SQLiteController sqlc) {
                Client = client;
                SQLC = sqlc;

                Message = msg;
                Channel = msg.Channel;
                Guild = Channel.Guild;
                User = Message.Author;
                if (Guild != null)
                    Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                Context = ctx;
            }

            public DiscordClient Client;
            public Players.SQLiteController SQLC;
        }
    }
}