using Dapper;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using static DiscordBotEthan.Players.SQLiteController;
using static DiscordBotEthan.Program;

namespace DiscordBotEthan.Commands {

    public class Query : BaseCommandModule {

        [Command("Query"), RequireOwner, Hidden]
        public async Task QueryCommand(CommandContext ctx, DiscordMember member) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            var output = await cnn.QueryAsync($"SELECT * FROM Players WHERE ID=@id", new { id = member.Id }).ConfigureAwait(false);

            if (!output.Any()) {
                await cnn.ExecuteAsync($"INSERT INTO Players (ID) VALUES (@id)", new { id = member.Id }).ConfigureAwait(false);
                await ctx.RespondAsync("This Member wasn't present in the DB until now");
                return;
            }

            Player player = new Player();
            foreach (var item in output) {
                long IDc = item.ID;
                string LastMessages = item.LastMessages;
                string Warns = item.Warns;
                long Muted = item.Muted;

                player = new Player {
                    ID = (ulong)IDc,
                    LastMessages = LastMessages.Split(",").ToList(),
                    Warns = string.IsNullOrWhiteSpace(Warns) ? new List<string>() : Warns.Split(",").ToList(),
                    Muted = Convert.ToBoolean(Muted)
                };
            }
            DiscordEmbedBuilder query = new DiscordEmbedBuilder {
                Title = $"Query | Returned",
                Description = @$"ID: {player.ID}
LastMessages: {string.Join(", ", player.LastMessages)}
Warns: {string.Join(", ", player.Warns)}
Muted: {player.Muted}",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: query);
        }
    }
}