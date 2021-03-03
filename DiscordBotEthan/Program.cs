using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBotEthan {

    internal class Program {
        public static DiscordClient discord;
        public static DiscordColor EmbedColor = new DiscordColor("#3299E0");
        public static readonly ulong MutedRole = 765286908133638204;
        public static readonly ulong LearnerRole = 734242782092329101;
        public static readonly string[] Statuses = { "Allah is watchin", "Despacito", "Fuck", "Janitor cleanup", "CSGO and Cheating", "EAC Bypass" };
        public static readonly string ConnString = @$"Data Source={Path.Join("Players","Players.db")}; Version=3;";

        private static void Main() {
            Console.WriteLine("Starting Checks");

            if (!File.Exists(Path.Join("Players", "Players.db"))) {
                Console.WriteLine("The Database is missing (Players.db)");
                Console.ReadLine();
                return;
            } else {
                Console.WriteLine("No Errors");
            }

            Console.WriteLine("Starting Bot");
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