using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.DataHandlers.SendPickBans
{
    internal class SendPickBanOperators
    {
        private readonly DiscordChannel _channel;

        public SendPickBanOperators(DiscordChannel channel)
        {
            _channel = channel;
        }


        public bool ContainsButton(ulong userId,
                                   string buttonId,
                                   out Models.Referee.StartPickBan? startPickBanOut,
                                   out Models.PickBans.PickBanDetailed? PickBanDetailedOut)
        {
            startPickBanOut = default;
            PickBanDetailedOut = default;

            bool contains = false;

            Func<Models.Referee.StartPickBan, bool> predicateFindVotingOperatiors = new(el => el.PickBanMap.PickBanDetailed.Any(el2 => el2.Operators.DetailedIds.Any(el3 => el3.Id == buttonId)));

            if (Models.Referee.StartPickBan.ListPickBans.Any(predicateFindVotingOperatiors))
            {
                Models.Referee.StartPickBan startPickBan = Models.Referee.StartPickBan.ListPickBans.First(predicateFindVotingOperatiors);

                Models.PickBans.PickBanDetailed detailedMap = startPickBan.PickBanMap.PickBanDetailed.First(el => el.Operators.DetailedIds.Any(el2 => el2.Id == buttonId));

                Models.PickBans.PickBanDetailedId detailedIdButton = detailedMap.Operators.DetailedIds.First(el => el.Id == buttonId);

                for (int i = 0; i < detailedMap.Operators.PickBanDetailed.Count; i++)
                {
                    if (!detailedMap.Operators.PickBanDetailed[i].Cheched)
                    {
                        Models.Teams.Team currentTeam = detailedMap.Operators.PickBanDetailed[i].Team ?? new Models.Teams.Team("");

                        //todo Отключена проверка на капитана
                        if (currentTeam.Capitan?.UserId == userId)
                        {
                            contains = true;

                            detailedMap.Operators.PickBanDetailed[i].PickBanName = detailedIdButton.Name;
                            detailedMap.Operators.PickBanDetailed[i].Cheched = true;

                            if (detailedMap.Operators.PickBanDetailed[i].PickBanType == Models.PickBanType.pick)
                            {
                                if (buttonId.StartsWith("Штурмовик"))
                                    detailedMap.Operators.RegPick(currentTeam, "Штурмовик", detailedIdButton.Name);
                                else if (buttonId.StartsWith("Поддержка"))
                                    detailedMap.Operators.RegPick(currentTeam, "Поддержка", detailedIdButton.Name);
                                else if (buttonId.StartsWith("Медик"))
                                    detailedMap.Operators.RegPick(currentTeam, "Медик", detailedIdButton.Name);
                                else if (buttonId.StartsWith("Снайпер"))
                                    detailedMap.Operators.RegPick(currentTeam, "Снайпер", detailedIdButton.Name);
                            }

                            detailedMap.Operators.DetailedIds.Remove(detailedIdButton);

                            startPickBanOut = startPickBan;
                            PickBanDetailedOut = detailedMap;
                        }
                        else
                        {
                            MessageQueue.Add(_channel.Id, $"Очередь голосования {currentTeam.Capitan?.UserId.GetLink()}", removeMessage: true);
                        }
                        break;
                    }
                }
            }

            return contains;
        }

        public async void SendPickBanMessage(Models.Referee.StartPickBan data,
                                             Models.PickBans.PickBanDetailed? detailedMap)
        {
            try
            {
                Models.Teams.Team? currentTeam = default;
                string linkCapitan = string.Empty;
                Models.PickBanType currentPickBanType = default;

                Models.PickBans.PickBanOperators operators = detailedMap!.Operators;

                for (int i = 0; i < operators.PickBanDetailed.Count; i++)
                {
                    if (!operators.PickBanDetailed[i].Cheched)
                    {
                        currentTeam = operators.PickBanDetailed[i].Team;
                        linkCapitan = currentTeam?.Capitan?.UserId.GetLink() ?? string.Empty;
                        currentPickBanType = operators.PickBanDetailed[i].PickBanType;
                        break;
                    }
                }

                Builders.MessageBuilder builderButton;

                Builders.ButtonOperators buttonOperators = new(_channel, detailedMap, operators, currentTeam, currentPickBanType);

                if (!operators.IsActive)
                    operators.DateStart = DateTime.Now;
                if (currentPickBanType == default)
                    operators.DateEnd = DateTime.Now;

                await buttonOperators.SendButtonByKeyClassOperators("Штурмовик");
                await buttonOperators.SendButtonByKeyClassOperators("Поддержка");
                await buttonOperators.SendButtonByKeyClassOperators("Медик");
                await buttonOperators.SendButtonByKeyClassOperators("Снайпер");

                if (currentPickBanType == default || currentTeam == default)
                    builderButton = CreateResultVoting(data, detailedMap.PickBanName ?? string.Empty, operators);
                else
                    builderButton = CreateMessageVoting(detailedMap, currentTeam, linkCapitan, currentPickBanType);

                if (detailedMap.ClassOperatorsMessages.ContainsKey("Description"))
                {
                    MessageQueue.Add(_channel.Id, builderButton.GetMessage(), detailedMap.ClassOperatorsMessages["Description"]);
                }
                else
                {
                    DiscordMessage message = await _channel.SendMessageAsync(builderButton.GetMessage());
                    detailedMap.ClassOperatorsMessages.Add("Description", message.Id);
                }
            }
            catch (Exception ex)
            {
                Worker.LogErr(ex.ToString());
            }

        }

        public static Builders.MessageBuilder CreateMessageVoting(Models.PickBans.PickBanDetailed detailedMap,
                                                                  Models.Teams.Team currentTeam,
                                                                  string linkCapitan,
                                                                  Models.PickBanType currentPickBanType)
        {
            Builders.MessageBuilder builderButton;

            string pickOrBanEmoji = currentPickBanType == Models.PickBanType.pick ? ":white_check_mark:" : ":x:";
            string pickOrBan = currentPickBanType == Models.PickBanType.ban ? "Бан" : "Пик";

            builderButton = new Builders.MessageBuilder()
                .AddDescription(
                    $"Карта: {detailedMap.PickBanName}.\n\n" +
                    $"{pickOrBanEmoji} {Formatter.Bold(pickOrBan)} оперативника от капитана команды {Formatter.Bold(currentTeam?.Name)} {linkCapitan}");

            return builderButton;
        }

        public static Builders.MessageBuilder CreateResultVoting(Models.Referee.StartPickBan data,
                                                                 string mapName,
                                                                 Models.PickBans.PickBanOperators operators)
        {
            Builders.MessageBuilder builderButton;
            Builders.Embeds embeds = new Builders.Embeds()
                .Init()
                .AddDescription(
                $"Голосование команд {Formatter.Bold(data.Team1Name)} и {Formatter.Bold(data.Team2Name)} по выбору оперативников завершено.\n" +
                $"Бои пройдут на карте {Formatter.Bold(mapName)} на следующих оперативниках:");

            StringBuilder builderColumnOperators = new();

            foreach (KeyValuePair<Models.Teams.Team, List<Models.PickBans.PickOperatorsData>> itemTeamOperators in operators.TeamOperators)
            {
                foreach (Models.PickBans.PickOperatorsData operatorsClass in itemTeamOperators.Value)
                    builderColumnOperators.AppendLine(operatorsClass.OperatorName);

                embeds.AddField(Formatter.Bold(itemTeamOperators.Key.Name), builderColumnOperators.ToString(), true);

                builderColumnOperators.Clear();
            }

            builderColumnOperators.Clear();

            builderButton = new Builders.MessageBuilder()
                .AddEmbed(embeds.GetEmbed());

            return builderButton;
        }

    }
}
