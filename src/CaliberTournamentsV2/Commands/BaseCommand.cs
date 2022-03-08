using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Commands
{
    internal class BaseCommand : BaseCommandModule
    {
        internal static async Task SendMessage(CommandContext ctx, string message)
            => await ctx.Channel.SendMessageAsync(message);
        internal static async Task SendMessage(CommandContext ctx, DiscordEmbed embed)
            => await ctx.Channel.SendMessageAsync(embed);
        internal static async Task SendMessage(CommandContext ctx, DiscordMessageBuilder builder)
            => await ctx.Channel.SendMessageAsync(builder);

        internal static async Task SendOKMessage(CommandContext ctx)
        {
            await SendMessage(ctx, "ok");
        }
        internal static async Task SendErrorMessage(CommandContext ctx)
        {
            await SendMessage(ctx, "error");
        }


        public override async Task AfterExecutionAsync(CommandContext ctx)
        {
            try
            {
                await ctx.Message.DeleteAsync("Processes");
            }
            catch (Exception ex)
            {
                Worker.LogErr(ex.ToString());
            }
        }
    }
}
