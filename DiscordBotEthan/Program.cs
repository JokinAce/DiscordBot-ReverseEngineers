﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;

namespace DiscordBotEthan {

    internal class Program {
        public static DiscordClient discord;
        public static DiscordColor EmbedColor = new DiscordColor("#3299E0");
        public static readonly ulong MutedRole = 765286908133638204;
        public static readonly ulong LearnerRole = 734242782092329101;
        public static readonly string[] Statuses = { "Allah is watchin", "Despacito", "Fuck", "Janitor cleanup", "CSGO and Cheating", "EAC Bypass" };

        private static void Main() {
            Console.WriteLine("Started");
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task MainAsync() {
            discord = new DiscordClient(new DiscordConfiguration {
                Token = "",
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Information,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.AllUnprivileged
            });

            CommandsNextExtension commands = discord.UseCommandsNext(new CommandsNextConfiguration() {
                StringPrefixes = new[] { "." }
            });

            discord.Ready += EventHandlers.Discord_Ready;
            discord.GuildMemberAdded += EventHandlers.Discord_GuildMemberAdded;
            discord.MessageCreated += EventHandlers.Discord_MessageCreated;
            commands.CommandErrored += EventHandlers.Commands_CommandErrored;

            commands.SetHelpFormatter<CustomHelpFormatter>();
            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }


        public class PlayerSystem {
            public List<string> LastMessages { get; set; }
            public List<string> Warns { get; set; }
            public bool Muted { get; set; }

            public static async Task<PlayerSystem> GetPlayer(ulong id) {
                if (!File.Exists($"./Players/{id}.json")) {
                    using FileStream DefaultStream = File.OpenRead("./Players/playertemplate.json");
                    return await JsonSerializer.DeserializeAsync<PlayerSystem>(DefaultStream);
                }

                using FileStream PlayersStream = File.OpenRead($"./Players/{id}.json");
                return await JsonSerializer.DeserializeAsync<PlayerSystem>(PlayersStream);
            }

            public async Task Save(ulong id) {
                await File.WriteAllTextAsync($"./Players/{id}.json", JsonSerializer.Serialize(this));
            }
        }

        public class CustomHelpFormatter : DefaultHelpFormatter {

            public CustomHelpFormatter(CommandContext ctx) : base(ctx) {
            }

            public override CommandHelpMessage Build() {
                EmbedBuilder.Color = EmbedColor;
                return base.Build();
            }
        }
    }
}