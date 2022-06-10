using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Commands
{
    internal class Referee : BaseCommand, ICommands
    {
        [Command("InviteTeams")]
        [Aliases("team", "тим")]
#pragma warning disable CA1822 // its ok
        internal async Task InviteTeams(CommandContext ctx, string teamName1, string teamName2)
        {
            try
            {
                if (!Access.IsReferee(ctx.User, "InviteTeams"))
                    return;

                if (!await CheckRegisteredTeams(ctx, teamName1, teamName2))
                    return;

                string message =
                    $"В счетную {ctx.Channel.GetLink()} " +
                    $"приглашаются капитаны {Models.Teams.Team.GetCommand(teamName1)?.LinkCapitan} и {Models.Teams.Team.GetCommand(teamName2)?.LinkCapitan}";

                await ctx.Channel.SendMessageAsync(message);

            }
            catch (Exception ex)
            {
                Worker.LogErr($"InviteTeams. {ex}");
            }
        }
#pragma warning restore CA1822 // its ok

        [Command("StartPickBanMap")]
        [Aliases("map", "карта")]
#pragma warning disable CA1822 // its ok
        internal async Task StartPickBanMap(CommandContext ctx, string teamName1, string teamName2, string mode = "")
        {
            try
            {
                if (!Access.IsReferee(ctx.User, "StartPickBanMap"))
                    return;

                if (string.IsNullOrWhiteSpace(mode))
                {
                    MessageQueue.Add(ctx.Channel, "Не указан режим bo1, bo3, bo5.", removeMessage: true);
                    return;
                }

                if (!await CheckRegisteredTeams(ctx, teamName1, teamName2))
                    return;

                Models.Referee.StartPickBan startPickBan = new(ctx.Channel.Id, ctx.User, teamName1, teamName2, mode);

                startPickBan.PickBanMap.DateStart = DateTime.Now;

                Models.Referee.StartPickBan.AddPickBan(startPickBan);

                startPickBan.FillPickBansMap();

                Builders.Embeds embeds = new Builders.Embeds()
                    .Init()
                    .AddDescription($"Начало голосования команд {Formatter.Bold(startPickBan.Team1Name)} и {Formatter.Bold(startPickBan.Team2Name)}");

                await ctx.Channel.SendMessageAsync(embeds.GetEmbed());

                DiscordMessage message = await ctx.Channel.SendMessageAsync("...");

                DataHandlers.SendPickBans.SendPickBanMap map = new(DataHandlers.DataHandler.GetAllMaps(), ctx.Channel, message);
                map.SendPickBanMessage(startPickBan);
            }
            catch (Exception ex)
            {
                Worker.LogErr($"InviteTeams. {ex}");
            }
        }
#pragma warning restore CA1822 // its ok

        [Command("StartPickBanOperators")]
        [Aliases("oper", "опер")]
#pragma warning disable CA1822 // its ok
        internal async Task StartOperators(CommandContext ctx, string teamName1, string teamName2)
#pragma warning restore CA1822 // its ok
        {
            try
            {
                if (!Access.IsReferee(ctx.User, "StartPickBanOperators"))
                    return;

                string message = string.Empty;

                Models.Referee.StartPickBan? pickBan = Models.Referee.StartPickBan.GetPickBan(teamName1, teamName2);

                if (pickBan != null)
                {
                    if (pickBan.PickBanMap.IsActive)
                        message = $"У команд ещё не завершено голосование карт.";
                    //else if (pickBan.PickBanMap.PickBanDetailed.Any(el => el.Operators.IsActive && !el.PickBanOperatorsIsEnded))
                    //    message = $"У команд есть не завершенное голосование по оперативникам";
                    else if (pickBan.PickBanMap.PickBanDetailed.Any(el => (el.PickBanType != Models.PickBanType.ban || el.PickBanType != Models.PickBanType.none) && !el.PickBanOperatorsIsEnded))
                    {
                        string? resultError = pickBan.FillPickBanOperators(Models.Teams.Team.GetCommand(teamName1), Models.Teams.Team.GetCommand(teamName2));

                        if (resultError == null)
                        {
                            DataHandlers.SendPickBans.SendPickBanOperators operators = new(ctx.Channel);
                            operators.SendPickBanMessage(
                                pickBan,
                                pickBan.PickBanMap.PickBanDetailed.First(el => el.PickBanType != Models.PickBanType.ban && !el.PickBanOperatorsIsEnded)
                                );
                        }
                        else
                            message = resultError ?? string.Empty;
                    }
                    else
                        message = $"У команд {Formatter.Bold(teamName1)} и {Formatter.Bold(teamName2)} голосование по оперативникам на картах завершено";
                }
                else
                    message = $"Не найдено голосований команд {Formatter.Bold(teamName1)} и {Formatter.Bold(teamName2)}";

                if (!string.IsNullOrEmpty(message))
                {
                    Worker.LogWarn(message);
                    await ctx.Channel.SendMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                Worker.LogErr($"InviteTeams. {ex}");
            }
        }

        [Command("ChangeCapitan")]
        [Aliases("cap", "капитан")]
#pragma warning disable CA1822 // its ok
        internal async Task ChangeCapitan(CommandContext ctx, string teamName, string newCapitan)
#pragma warning restore CA1822 // its ok
        {
            try
            {
                if (!Access.IsReferee(ctx.User, "ChangeCapitan"))
                    return;

                if (!Models.Teams.Team.TeamIsRegistered(teamName))
                {
                    await ctx.Channel.SendMessageAsync($"Команда не зарегистрирована {teamName}");
                    return;
                }

                if (Models.Teams.Team.CapitanIsRegistered(newCapitan.GetID()))
                {
                    Models.Teams.Team? currentTeam = Models.Teams.Team.GetCommand(el => el.Capitan?.UserId == newCapitan.GetID());
                    await ctx.Channel.SendMessageAsync($"Капитан уже состоит в команде {currentTeam?.Name}");
                    return;
                }


                Models.Teams.Team? teamNewCapitan = Models.Teams.Team.GetCommand(teamName);

                if (teamNewCapitan != null)
                {
                    string previousCapitan = $"{teamNewCapitan?.Capitan?.Name}#{teamNewCapitan?.Capitan?.Discriminator}";

#pragma warning disable CS8602 // it is impossible to get null
                    teamNewCapitan.Capitan = new Models.Teams.Player(newCapitan.GetID());
#pragma warning restore CS8602 // it is impossible to get null

                    string currentCapitan = $"{teamNewCapitan.Capitan.Name}#{teamNewCapitan.Capitan.Discriminator}";

                    MessageQueue.Add(ctx.Channel.Id, $"Новым капитаном команды {teamNewCapitan.Name} является {currentCapitan}");

                    Worker.LogWarn($"Изменен капитан команды {teamNewCapitan.Name} с {previousCapitan} на {currentCapitan}");
                }
            }
            catch (Exception ex)
            {
                Worker.LogErr($"ChangeCapitan. {ex}");
            }
        }

        [Command("Coin")]
        [Aliases("c", "монетка")]
        internal async Task Coin(CommandContext ctx, string team1, string team2)
        {
            try
            {
                if (!Access.IsReferee(ctx.User, "Coin"))
                    return;

                if (!await CheckRegisteredTeams(ctx, team1, team2))
                    return;

                bool trueEagle = new Random().Next(0, 100) % 2 == 0;

                string message = $"Выпал{(trueEagle ? $" {Formatter.Bold("орел")}" : $"а {Formatter.Bold("решка")}")}.\n"
                    + $"Первым делает выбор команда {Formatter.Bold(trueEagle ? team1 : team2)}. " +
                    $"Капитан: {Formatter.Bold((trueEagle ? Models.Teams.Team.GetCommand(team1)?.LinkCapitan : Models.Teams.Team.GetCommand(team2)?.LinkCapitan))}";

                string color = trueEagle ? "#0035ff" : "#ffaf00";

                Builders.Embeds embed = new Builders.Embeds()
                    .Init(color: color)
                    .AddDescription(message);

                await ctx.Channel.SendMessageAsync(embed.GetEmbed());
            }
            catch (Exception ex)
            {
                Worker.LogErr($"InviteTeams. {ex}");
            }
        }

        private static async Task<bool> CheckRegisteredTeams(CommandContext ctx, string teamName1, string teamName2)
        {
            bool error = false;

            if (!Models.Teams.Team.TeamIsRegistered(teamName1))
            {
                error = true;
                await ctx.Channel.SendMessageAsync($"Команда не зарегистрирована {teamName1}");
            }
            if (!Models.Teams.Team.TeamIsRegistered(teamName2))
            {
                error = true;
                await ctx.Channel.SendMessageAsync($"Команда не зарегистрирована {teamName2}");
            }

            return !error;
        }
    }
}
