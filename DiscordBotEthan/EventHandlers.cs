using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using JokinsCommon;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using static DiscordBotEthan.Program;

namespace DiscordBotEthan {

    public static class EventHandlers {

        public static Task Discord_Ready(DiscordClient dc, DSharpPlus.EventArgs.ReadyEventArgs args) {
            _ = Task.Run(async () => {
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

                var PS = await Program.PlayerSystem.GetPlayer(args.Author.Id);
                if (PS.LastMessages.Count > 1 && PS.LastMessages.Contains(args.Message.Content)) {
                    await Misc.Warn(args.Channel, args.Author, "Spamming");
                    PS.LastMessages.Clear();
                } else if (PS.LastMessages.Contains(args.Message.Content)) {
                    PS.LastMessages.Add(args.Message.Content);
                } else {
                    PS.LastMessages.Clear();
                    PS.LastMessages.Add(args.Message.Content);
                }
                await PS.Save(args.Author.Id);

                if (new Random().Next(500) == 1) {
                    using WebClient client = new WebClient();

                    await new DiscordMessageBuilder()
                        .WithContent(client.DownloadString("https://insult.mattbas.org/api/insult"))
                        .WithReply(args.Message.Id)
                        .SendAsync(args.Channel);
                }

                if (args.Message.Attachments.Count > 0) {
                    foreach (var attach in args.Message.Attachments) {
                        if (attach.FileName.Contains("exe")) {
                            await args.Message.DeleteAsync("EXE File");
                            await Misc.Warn(args.Channel, args.Author, "Uploading a EXE File");
                        }
                    }
                } else if (args.Message.Content.RemoveString(new string[] { " ", "." }).ToLower().Contains("discordgg")) {
                    await args.Message.DeleteAsync();
                    await Misc.Warn(args.Channel, args.Author, "Invite Link");
                } else if (args.Message.Content.ToLower().Contains("nigger") || args.Message.Content.ToLower().Contains("nigga")) {
                    await Misc.Warn(args.Channel, args.Author, "Saying the N-Word");

                    await new DiscordMessageBuilder()
                        .WithContent("Keep up the racism and you will get banned\nUse nig, nibba instead atleast")
                        .WithReply(args.Message.Id, true)
                        .SendAsync(args.Channel);
                    //await args.Channel.SendMessageAsync("Keep up the racism and you will get banned\nUse nig, nibba instead atleast");
                }
            });

            return Task.CompletedTask;
        }

        public static async Task Discord_GuildMemberAdded(DiscordClient dc, DSharpPlus.EventArgs.GuildMemberAddEventArgs args) {
            await args.Member.GrantRoleAsync(args.Guild.GetRole(LearnerRole));

            var PS = await PlayerSystem.GetPlayer(args.Member.Id);
            if (PS.Muted) {
                _ = Task.Run(async () => {
                    try {
                        DiscordRole MutedRole = args.Guild.GetRole(Program.MutedRole);
                        await args.Member.GrantRoleAsync(MutedRole);
                        await Task.Delay(86400000);
                        var PS = await PlayerSystem.GetPlayer(args.Member.Id);
                        PS.Muted = false;
                        await PS.Save(args.Member.Id);
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
                    //await args.Context.RespondAsync("Idk what the fuck you want to do with that Command (Arguments are faulty)");

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

                    //await args.Context.RespondAsync("The FBI has been contacted (You don't have **the** (Thx Sven for correction) rights for that **c**ommand (Another correction))");
                    break;
            }
        }
    }
}