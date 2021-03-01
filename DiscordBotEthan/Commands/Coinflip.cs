using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Coinflip : BaseCommandModule {

        [Command("CoinFlip"), Description("Flips a Coin, returns Heads or Tails")]
        public async Task CoinFlipCommand(CommandContext ctx) {
            if (new Random().Next(2) == 1) {
                await ctx.RespondAsync("Tails");
            } else {
                await ctx.RespondAsync("Heads");
            }
        }
    }
}