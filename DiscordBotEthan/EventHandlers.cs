using Dapper;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using JokinsCommon;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SQLite;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using static DiscordBotEthan.Program;

namespace DiscordBotEthan {

    public static class EventHandlers {

        public static Task Discord_Ready(DiscordClient dc, DSharpPlus.EventArgs.ReadyEventArgs args) {
            _ = Task.Run(async () => {
                using IDbConnection cnn = new SQLiteConnection(ConnString);
                var output = await cnn.QueryAsync("SELECT * FROM Reminders");
                var Guild = await dc.GetGuildAsync(GuildID);

                dc.Logger.LogInformation("Looking for Reminders");
                if (output.Any()) {
                    foreach (var item in output) {
                        long ID = item.ID;
                        long ChannelID = item.ChannelID;
                        long Date = item.Date;
                        string Reminder = item.Reminder;

                        DiscordMember member = await Guild.GetMemberAsync((ulong)ID);
                        DiscordChannel channel = Guild.GetChannel((ulong)ChannelID);
                        DateTime dateTime = DateTime.FromBinary(Date);

                        if (dateTime < DateTime.Now) {
                            await cnn.ExecuteAsync("DELETE FROM Reminders WHERE Date=@date", new { date = Date });
                            await channel.SendMessageAsync($":alarm_clock:, {member.Mention} you wanted me to remind you the following but I'm Late:\n\n{Reminder}");
                            continue;
                        }

                        _ = Task.Run(async () => {
                            await Task.Delay((int)dateTime.Subtract(DateTime.Now).TotalMilliseconds);
                            await channel.SendMessageAsync($":alarm_clock:, {member.Mention} you wanted me to remind you the following:\n\n{Reminder}");

                            using IDbConnection cnn = new SQLiteConnection(ConnString);
                            await cnn.ExecuteAsync("DELETE FROM Reminders WHERE Date=@date", new { date = Date });
                        });
                    }
                    dc.Logger.LogInformation("Found Reminders and started them");
                } else {
                    dc.Logger.LogInformation("No Reminders found");
                }

                while (true) {
                    foreach (var Status in Statuses) {
                        DiscordActivity activity = new DiscordActivity {
                            ActivityType = ActivityType.Playing,
                            Name = Status
                        };
                        await dc.UpdateStatusAsync(activity, UserStatus.DoNotDisturb);
                        dc.Logger.LogInformation("Status Update");
                        await Task.Delay(120000);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public static Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs args) {
            _ = Task.Run(async () => {
                if (args.Author.IsBot)
                    return;

                var PS = await Players.SQLiteController.GetPlayer(args.Author.Id);
                if (PS.LastMessages.Count > 1 && PS.LastMessages.Contains(args.Message.Content)) {
                    await Misc.Warn(args.Channel, args.Author, "Spamming");
                    PS.Warns.Add("Spamming");
                    PS.LastMessages.Clear();
                } else if (PS.LastMessages.Contains(args.Message.Content)) {
                    PS.LastMessages.Add(args.Message.Content);
                } else {
                    PS.LastMessages.Clear();
                    PS.LastMessages.Add(args.Message.Content);
                }
                await PS.Save();

                if (new Random().Next(500) == 1) {
                    using WebClient client = new WebClient();

                    await new DiscordMessageBuilder()
                        .WithContent(client.DownloadString("https://insult.mattbas.org/api/insult"))
                        .WithReply(args.Message.Id)
                        .SendAsync(args.Channel);
                }

                string stripped = args.Message.Content.RemoveString(" ", ".").ToLower();

                if (args.Message.Attachments.Count > 0) {
                    foreach (var attach in args.Message.Attachments) {
                        if (attach.FileName.EndsWith("exe")) {
                            await args.Message.DeleteAsync("EXE File");
                            await Misc.Warn(args.Channel, args.Author, "Uploading a EXE File");
                        } else if (attach.FileName.EndsWith("dll")) {
                            await args.Message.DeleteAsync("DLL File");
                            await Misc.Warn(args.Channel, args.Author, "Uploading a DLL File");
                        }
                    }
                } else if (stripped.Contains("discordgg")) {
                    await args.Message.DeleteAsync();
                    await Misc.Warn(args.Channel, args.Author, "Invite Link");
                } else if (stripped.Contains("nigger") || stripped.Contains("nigga")) {
                    await Misc.Warn(args.Channel, args.Author, "Saying the N-Word");

                    await new DiscordMessageBuilder()
                        .WithContent("Keep up the racism and you will get banned\nUse nig, nibba instead atleast")
                        .WithReply(args.Message.Id, true)
                        .SendAsync(args.Channel);
                }
            });

            return Task.CompletedTask;
        }

        public static async Task Discord_GuildMemberAdded(DiscordClient dc, DSharpPlus.EventArgs.GuildMemberAddEventArgs args) {
            await args.Member.GrantRoleAsync(args.Guild.GetRole(LearnerRole));

            var PS = await Players.SQLiteController.GetPlayer(args.Member.Id);
            if (PS.Muted) {
                _ = Task.Run(async () => {
                    try {
                        DiscordRole MutedRole = args.Guild.GetRole(Program.MutedRole);
                        await args.Member.GrantRoleAsync(MutedRole);
                        await Task.Delay(86400000);
                        var PS = await Players.SQLiteController.GetPlayer(args.Member.Id);
                        PS.Muted = false;
                        await PS.Save();
                        await args.Member.RevokeRoleAsync(MutedRole);
                    } catch (Exception) {
                        dc.Logger.LogInformation($"Failed the Mute Bypass detection process for {args.Member.Mention}");
                    }
                });
            }
        }

        public static async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs args) {
            switch (args.Exception) {
                case ArgumentException e:

                    await new DiscordMessageBuilder()
                        .WithContent($"Idk what the fuck you want to do with that Command (Argument {e.ParamName ?? "unknown"} is faulty)")
                        .WithReply(args.Context.Message.Id, true)
                        .SendAsync(args.Context.Channel);
                    break;

                case DSharpPlus.CommandsNext.Exceptions.ChecksFailedException _:
                    await new DiscordMessageBuilder()
                        .WithContent("The FBI has been contacted (You don't have the rights for that command)")
                        .WithReply(args.Context.Message.Id, true)
                        .SendAsync(args.Context.Channel);

                    break;
            }
        }
    }
}