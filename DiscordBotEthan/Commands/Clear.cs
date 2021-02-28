using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Clear : BaseCommandModule {

        [Command("Clear"), RequirePermissions(DSharpPlus.Permissions.Administrator), Description("Removes a amount of messages or recreates the Channel")]
        public async Task ClearCommand(CommandContext ctx, [Description("Amount or all")] string amount) {
            if (int.TryParse(amount, out int parsed)) {
                try {
                    var Messages = await ctx.Channel.GetMessagesAsync(parsed + 1);
                    await ctx.Channel.DeleteMessagesAsync(Messages);

                    var msg = await ctx.RespondAsync($"{amount} messages deleted");
                    await Task.Delay(3000);
                    await msg.DeleteAsync();
                } catch (DSharpPlus.Exceptions.BadRequestException) {
                    await ctx.RespondAsync("Messages are older then 14 Days\nDiscord API no like do .Clear all instead");
                }
            } else if (amount.Equals("all", StringComparison.OrdinalIgnoreCase)) {
                try {
                    var TempPos = ctx.Channel.Position;
                    var NewCh = await ctx.Channel.CloneAsync();
                    await ctx.Channel.DeleteAsync();
                    await NewCh.ModifyPositionAsync(TempPos);
                } catch (Exception) {
                    await ctx.RespondAsync("Something horrible happend, Command faulted");
                }
            } else {
                throw new ArgumentException("Argument is missing or is invalid", "amount");
            }
        }
    }
}