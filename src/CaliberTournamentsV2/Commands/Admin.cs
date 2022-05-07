using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Commands
{
    internal class Admin : BaseCommand, ICommands
    {
        [Command("RegisterTeam")]
        [Aliases("regteam", "рег")]
#pragma warning disable CA1822 // its ok
        internal async Task RegisterTeam(CommandContext ctx, string teamName, string capitan)
#pragma warning restore CA1822 // its ok
        {
            try
            {
                if (!Access.IsAdmin(ctx.User, "RegisterTeam"))
                    return;

                await ctx.Channel.SendMessageAsync(AddTeam(teamName, capitan));

            }
            catch (Exception ex)
            {
                Worker.LogErr($"InviteTeams. {ex}");
            }
        }

        [Command("LoadTeams")]
        [Aliases("добавить")]
#pragma warning disable CA1822 // its ok
        internal async Task LoadTeams(CommandContext ctx)
#pragma warning restore CA1822 // its ok
        {
            try
            {
                if (!Access.IsAdmin(ctx.User, "LoadTeams"))
                    return;

                GoogleApis.Spreadsheets spreadsheets = new();

                List<Tuple<string, string>> commands = GoogleApis.Spreadsheets.LoadCommands();

                StringBuilder builderResult = new();

                int addedCommand = 0;
                foreach (Tuple<string, string> item in commands)
                {
                    if (ulong.TryParse(item.Item2, out ulong id))
                    {
                        builderResult.AppendLine(AddTeam(item.Item1, item.Item2));
                        addedCommand++;
                    }
                }

                if (builderResult.Length > 2000)
                    await ctx.Channel.SendMessageAsync("Добавлено команд: " + addedCommand);
                else
                    await ctx.Channel.SendMessageAsync(builderResult.ToString());

                builderResult.Clear();
            }
            catch (Exception ex)
            {
                Worker.LogErr($"InviteTeams. {ex}");
            }
        }

        [Command("GetRegisteredTeams")]
        [Aliases("gregteams", "список")]
        internal async Task GetRegisteredTeams(CommandContext ctx, string userOrTeam = "")
        {
            try
            {
                if (!Access.IsAdmin(ctx.User, "GetRegisteredTeams"))
                    return;

                Builders.Embeds embedsRegTeams = new Builders.Embeds()
                    .Init()
                    .AddDescription("Зарегистрированные команды");

                StringBuilder builerFileds = new();

                int countTeamInField = 0;
                int totalCountTeams = 0;

                bool findUser = false;
                Models.Teams.Player capitan = default;
                bool findTeam = false;

                if (!string.IsNullOrWhiteSpace(userOrTeam))
                {
                    if (userOrTeam.IsIDUser())
                    {
                        capitan = new(userOrTeam.GetID());
                        findUser = true;
                    }
                    else
                        findTeam = true;
                }

                foreach (Models.Teams.Team itemTeam in Models.Teams.Team.Teams)
                {
                    if (findUser && (!itemTeam.Capitan?.UserId.Equals(capitan?.UserId) ?? false))
                        continue;
                    if (findTeam && !itemTeam.Name.Contains(userOrTeam, StringComparison.OrdinalIgnoreCase))
                        continue;


                    if (totalCountTeams % 10 == 0)
                    {
                        embedsRegTeams.AddField(
                            $"{totalCountTeams - countTeamInField + 1}/{totalCountTeams}",
                            builerFileds.ToString(), true);
                        builerFileds.Clear();

                        countTeamInField = 0;
                    }

                    builerFileds.Append(itemTeam.Name);
                    builerFileds.Append(" - ");
                    builerFileds.AppendLine(itemTeam.Capitan?.Name);

                    countTeamInField++;
                    totalCountTeams++;
                }

                if (builerFileds.Length != 0)
                {
                    embedsRegTeams.AddField(
                        $"{totalCountTeams - countTeamInField + 1}/{totalCountTeams}",
                        builerFileds.ToString(), true);
                    builerFileds.Clear();
                }

                await ctx.Channel.SendMessageAsync(embedsRegTeams.GetEmbed());
            }
            catch (Exception ex)
            {
                Worker.LogErr($"GetRegisteredTeams. {ex}");
            }
        }

        [Command("RegisterReferee")]
        [Aliases("regreferee", "судья")]
#pragma warning disable CA1822 // its ok
        internal async Task RegisterReferee(CommandContext ctx, string referee)
#pragma warning restore CA1822 // its ok
        {
            try
            {
                if (!Access.IsAdmin(ctx.User, "RegisterReferee"))
                    return;

                ulong refereeID = referee.GetID();

                Access.RegisterReferee(refereeID);

                DiscordUser userReferee = await Bot.DiscordBot.GetDiscordUserById(refereeID);

                await ctx.Channel.SendMessageAsync("Зарегистрирован судья: " + userReferee.GetLink());
            }
            catch (Exception ex)
            {
                Worker.LogErr(ex.ToString());
            }
        }

        [Command("GetInfo")]
        internal async Task GetInfo(CommandContext ctx, string user)
        {
            try
            {
                if (!Access.IsAdmin(ctx.User, "GetInfo"))
                    return;

                ulong userId = user.GetID();

                DiscordUser dataUser = await Bot.DiscordBot.GetDiscordUserById(userId);

                StringBuilder sb = new();
                sb.Append(dataUser.Username);
                sb.Append("#");
                sb.AppendLine(dataUser.Discriminator);
                sb.Append("Id: ");
                sb.AppendLine(dataUser.Id.ToString());
                sb.AppendLine("Avatar: ");
                sb.AppendLine(dataUser.AvatarUrl);

            }
            catch (Exception ex)
            {
                Worker.LogErr(ex.ToString());
            }
        }

        [Command("Stats")]
        [Aliases("стат")]
        internal async Task GetStatistics(CommandContext ctx, string type = "", string details = "")
        {
            try
            {
                if (!Access.IsAdmin(ctx.User, "GetStatistics"))
                    return;

                string message = "";
                bool isError = true;

                if (string.IsNullOrWhiteSpace(type))
                {
                    StringBuilder sb = new("Доступные значения:");
                    sb.AppendLine("operators (all, picked, banned)");
                    sb.AppendLine("maps (\"\", picked, banned)");

                    message = sb.ToString();
                }
                else if (type != "operators" && type != "maps")
                {
                    message = "Ошибка доступных значений";
                }
                else
                {
                    Models.StatisticTypes enumType = (Models.StatisticTypes)Enum.Parse(typeof(Models.StatisticTypes), type);
                    Models.StatisticDetailedTypes enumDetails = string.IsNullOrWhiteSpace(details)
                        ? Models.StatisticDetailedTypes.none
                        : (Models.StatisticDetailedTypes)Enum.Parse(typeof(Models.StatisticDetailedTypes), details);

                    DiscordEmbed embedMessage = new Statistics().GetStatistics(enumType, enumDetails);

                    await ctx.Channel.SendMessageAsync(embedMessage);

                    isError = false;
                }

                if (isError)
                    MessageQueue.Add(ctx.Channel, message, removeMessage: true);
            }
            catch (Exception ex)
            {
                Worker.LogErr(ex.ToString());
            }
        }

        private static string AddTeam(string teamName, string capitan)
        {
            string message = string.Empty;

            if (Models.Teams.Team.TeamIsRegistered(teamName))
                message = $"Команда {teamName} уже зарегистрирована";
            else
            {
                Models.Teams.Team? newTeam = default;

                try
                {
                    newTeam = new(
                        teamName,
                        new Models.Teams.Player(capitan.GetID()));

                }
                catch (ArgumentNullException ex)
                {
                    message = ex.Message;
                }

                if (newTeam != null)
                {
                    Models.Teams.Team.AddTeam(newTeam);

                    message = $"Добавлена команда {Formatter.Bold(teamName)}. Капитан: {Formatter.Bold(newTeam.Capitan?.Name)}#{Formatter.Bold(newTeam.Capitan?.Discriminator)}";
                }

                Worker.LogInf(message);

            }

            return message;
        }
    }
}