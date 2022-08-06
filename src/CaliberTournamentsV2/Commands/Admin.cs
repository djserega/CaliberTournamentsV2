using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Text;

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
                Worker.LogErr($"RegisterTeam. {ex}");
            }
        }

        [Command("LoadTeams")]
        [Aliases("добавить", "lt")]
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

                if (builderResult.Length > 1000)
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
                Worker.LogErr($"RegisterReferee. {ex}");
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
                Worker.LogErr($"GetInfo. {ex}");
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
                    StringBuilder sb = new("Доступные значения:\n");
                    sb.AppendLine("operators (picked, banned)");
                    sb.AppendLine("maps (\"\", picked, banned)");
                    sb.AppendLine("mapsAndOperators (\"\", picked, banned)");

                    message = sb.ToString();
                }
                else if (type != "operators" && type != "maps" && type != "mapsAndOperators")
                {
                    message = "Ошибка доступных значений";
                }
                else
                {
                    Models.StatisticTypes enumType = (Models.StatisticTypes)Enum.Parse(typeof(Models.StatisticTypes), type);
                    Models.StatisticDetailedTypes enumDetails = string.IsNullOrWhiteSpace(details)
                        ? Models.StatisticDetailedTypes.none
                        : (Models.StatisticDetailedTypes)Enum.Parse(typeof(Models.StatisticDetailedTypes), details);

                    DiscordEmbed[] embedMessages = new Statistics().GetStatistics(enumType, enumDetails);

                    if (embedMessages.Length > 1)
                    {
                        Builders.MessageBuilder builder = new();
                        foreach (DiscordEmbed item in embedMessages)
                            builder.AddEmbed(item);
                    }
                    else
                        await ctx.Channel.SendMessageAsync(embedMessages[0]);

                    isError = false;
                }

                if (isError)
                    MessageQueue.Add(ctx.Channel, message, removeMessage: true);
            }
            catch (Exception ex)
            {
                Worker.LogErr($"GetStatistics. {ex}");
            }
        }

        [Command("GetVotingStatus")]
        [Aliases("голосования")]
        internal async Task GetVotingStatus(CommandContext ctx)
        {
            try
            {
                if (!Access.IsAdmin(ctx.User, "GetVotingStatus"))
                    return;

                Builders.Embeds embed = new Builders.Embeds()
                    .Init()
                    .AddDescription("Статус голосования команд");

                StringBuilder sb = new();
                sb.Append("Всего команд:");
                sb.AppendLine(Models.Referee.StartPickBan.ListPickBans.Count.ToString());

                sb.Append("Запущено голосований:");
                sb.AppendLine(Models.Referee.StartPickBan.ListPickBans.Where(el => el.PickBanMap.IsActive).Count().ToString());

                sb.Append("Завершено голосований:");
                sb.AppendLine(Models.Referee.StartPickBan.ListPickBans.Where(el => el.PickBanMap.IsEnded).Count().ToString());

                embed.AddField("Детали", sb.ToString());

                MessageQueue.Add(ctx.Channel.Id, embed: embed.GetEmbed());
            }
            catch (Exception ex)
            {
                Worker.LogErr($"GetVotingStatus. {ex}");
            }
        }

        [Command("GetDetailedVotingTeams")]
        [Aliases("детали")]
        internal async Task GetDetailedVotingTeams(CommandContext ctx, string team1 = "", string team2 = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(team1) || string.IsNullOrWhiteSpace(team2))
                    return;

                if (!Access.IsAdmin(ctx.User, "GetDetailedVotingTeams"))
                    return;

                IEnumerable<Models.Referee.StartPickBan>? pickBans = Models.Referee.StartPickBan.GetPickBans(team1, team2);
                if (pickBans == default)
                {
                    MessageQueue.Add(ctx.Channel, $"Голосований команд {Formatter.Bold(team1)} и {Formatter.Bold(team2)} не найдено.");
                    return;
                }

                Builders.Embeds embed = new Builders.Embeds()
                    .Init()
                    .AddDescription($"Голосования команд {team1} и {team2}");


                StringBuilder sb = new();

                foreach (Models.Referee.StartPickBan itemPickBan in pickBans)
                {
                    embed.AddField("Основная информация", itemPickBan.GetMainInfo(sb), true);
                    embed.AddField("Голосование по картам", itemPickBan.PickBanMap.GetMainInfo(sb), true);
                    embed.AddField("\u200b", "\u200b");
                }

                MessageQueue.Add(ctx.Channel.Id, embed.GetEmbed());
            }
            catch (Exception ex)
            {
                Worker.LogErr($"GetDetailedVotingTeams. {ex}");
            }
        }

        [Command("RemoveVotingOperators")]
        [Aliases("удалить")]
        internal async Task RemoveVotingOperators(CommandContext ctx, string team1 = "", string team2 = "", int idPickban = 99999, int idMap = 99999)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(team1) || string.IsNullOrWhiteSpace(team2) || idPickban == 99999 || idMap == 99999)
                    return;

                if (!Access.IsAdmin(ctx.User, "RemoveVotingOperators"))
                    return;

                IEnumerable<Models.Referee.StartPickBan>? pickBans = Models.Referee.StartPickBan.GetPickBans(team1, team2);
                if (pickBans == default)
                {
                    MessageQueue.Add(ctx.Channel, $"Голосований команд {Formatter.Bold(team1)} и {Formatter.Bold(team2)} не найдено.");
                    return;
                }

                if (!pickBans.Any(el => el.Id == idPickban))
                {
                    MessageQueue.Add(ctx.Channel, $"Не найдено id голосования {Formatter.Bold(idPickban.ToString())}.");
                    return;
                }

                Models.Referee.StartPickBan pickBan = pickBans.First(el => el.Id == idPickban);

                if (!pickBan.PickBanMap.PickBanDetailed.Any(el => el.Id == idMap))
                {
                    MessageQueue.Add(ctx.Channel, $"Не найдено id карты {Formatter.Bold(idMap.ToString())}.");
                    return;
                }

                Models.PickBans.PickBanDetailed pickBanDetailed = pickBan.PickBanMap.PickBanDetailed.First(el => el.Id == idMap);



                Builders.Embeds embed = new Builders.Embeds()
                    .Init()
                    .AddDescription($"Отмена голосования оперативников на карте {pickBanDetailed.PickBanName} от {team1} и {team2}");

                Builders.MessageBuilder builder = new Builders.MessageBuilder()
                    .AddEmbed(embed.GetEmbed());
                builder.AddButton("Удалить голосование", false, ButtonStyle.Danger, $"removeVoting_{idPickban}_{idMap}_accept");
                builder.AddButton("Отмена", false, ButtonStyle.Primary, $"removeVoting_{idPickban}_{idMap}_rejected");


                MessageQueue.Add(ctx.Channel.Id, builder.GetMessage());
            }
            catch (Exception ex)
            {
                Worker.LogErr($"RemoveVotingOperators. {ex}");
            }
        }

        private static string AddTeam(string teamName, string capitan)
        {
            string message = string.Empty;
            string logMessage = string.Empty;

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
                    message = "Возникла ошибка...";
                    logMessage = ex.Message;
                    Worker.LogErr(logMessage);
                }

                if (newTeam != null)
                {
                    Models.Teams.Team.AddTeam(newTeam);

                    message = $"Добавлена команда {Formatter.Bold(teamName)}. Капитан: {Formatter.Bold(newTeam.Capitan?.Name)}#{Formatter.Bold(newTeam.Capitan?.Discriminator)}";
                    logMessage = $"Add command {teamName}. Capitan {newTeam.Capitan?.Name}#{newTeam.Capitan?.Discriminator}";

                    Worker.LogInf(logMessage);
                }
            }

            return message;
        }
    }
}