using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.DataHandlers.SendPickBans
{
    internal class SendPickBanMap
    {
        private readonly string[] _allMaps;
        private readonly DiscordChannel _channel;
        private readonly DiscordMessage _message;

        public SendPickBanMap(string[] allMaps,
                              DiscordChannel channel,
                              DiscordMessage message)
        {
            _allMaps = allMaps;
            _channel = channel;
            _message = message;
        }


        public bool ContainsButton(ulong userId,
                                   string buttonId,
                                   out Models.Referee.StartPickBan? startPickBanOut)
        {
            startPickBanOut = default;

            bool contains = false;

            Func<Models.Referee.StartPickBan, bool> predicateFindVotingByButtonId = new(el => el.PickBanMap.DetailedIds.Any(el => el.Id == buttonId));

            if (Models.Referee.StartPickBan.ListPickBans.Any(predicateFindVotingByButtonId))
            {
                Models.Referee.StartPickBan startPickBan = Models.Referee.StartPickBan.ListPickBans.First(predicateFindVotingByButtonId);

                Models.PickBans.PickBanDetailedId detailedIdButton = startPickBan.PickBanMap.DetailedIds.First(el => el.Id == buttonId);

                for (int i = 0; i < startPickBan.PickBanMap.PickBanDetailed.Count; i++)
                {
                    if (!startPickBan.PickBanMap.PickBanDetailed[i].Cheched)
                    {
                        if (startPickBan.PickBanMap.PickBanDetailed[i].Team?.Capitan == null)
                            break;

                        if (startPickBan.PickBanMap.PickBanDetailed[i].Team?.Capitan?.UserId == userId
                            || Access.IsReferee(userId))
                        {
                            contains = true;

                            startPickBan.PickBanMap.PickBanDetailed[i].PickBanName = detailedIdButton.Name;
                            startPickBan.PickBanMap.PickBanDetailed[i].Cheched = true;

                            startPickBan.PickBanMap.DetailedIds.Remove(detailedIdButton);

                            startPickBanOut = startPickBan;
                        }
                        else
                        {
                            MessageQueue.Add(
                                _channel.Id,
                                $"Очередь голосования {startPickBan.PickBanMap.PickBanDetailed[i].Team?.Capitan?.UserId.GetLink()}",
                                removeMessage: true);
                        }
                        break;
                    }
                }
            }

            return contains;
        }

        public void SendPickBanMessage(Models.Referee.StartPickBan data)
        {
            Models.Teams.Team? currentTeam = default;
            string linkCapitan = string.Empty;

            Models.PickBanType currentPickBanType = default;
            for (int i = 0; i < data.PickBanMap.PickBanDetailed.Count; i++)
            {
                if (!data.PickBanMap.PickBanDetailed[i].Cheched)
                {
                    currentTeam = data.PickBanMap.PickBanDetailed[i].Team;
                    linkCapitan = data.PickBanMap.PickBanDetailed[i].Team?.Capitan?.UserId.GetLink() ?? string.Empty;
                    currentPickBanType = data.PickBanMap.PickBanDetailed[i].PickBanType;
                    break;
                }
            }

            Builders.MessageBuilder builderButton;

            if (currentPickBanType == default || currentTeam == default)
                builderButton = CreateResultVoting(data);
            else
                builderButton = CreateMessageVoting(data, currentTeam, linkCapitan, currentPickBanType);

            bool detailedIdsChanged = AddButtonPickBanMessage(data.PickBanMap, builderButton);

            if (detailedIdsChanged)
            {
                if (builderButton.Buttons.Where(el => el.Disabled).Count() == 4)
                {
                    IEnumerable<Builders.ButtonModel> buttons = builderButton.Buttons.Where(el => !el.Disabled);
                    for (int i = 0; i < buttons.Count(); i++)
                    {
                        buttons.ElementAt(i).Disabled = true;
                    }
                }

                MessageQueue.Add(_channel.Id, builderButton.GetMessage(), _message.Id);
            }
        }

        public static Builders.MessageBuilder CreateMessageVoting(Models.Referee.StartPickBan data,
                                                                  Models.Teams.Team currentTeam,
                                                                  string linkCapitan,
                                                                  Models.PickBanType currentPickBanType)
        {
            Builders.MessageBuilder builderButton;
            string modeString = data.Mode switch
            {
                Models.PickBanMode.bestOf1 => "Best of 1",
                Models.PickBanMode.bestOf3 => "Best of 3",
                Models.PickBanMode.bestOf5 => "Best of 5",
                _ => "<none>"
            };

            string pickOrBanEmoji = currentPickBanType == Models.PickBanType.pick ? ":white_check_mark:" : ":x:";
            string pickOrBan = currentPickBanType == Models.PickBanType.ban ? "Бан" : "Пик";

            builderButton = new Builders.MessageBuilder()
                .AddDescription(
                    $"Режим {modeString}\n" +
                    $"{pickOrBanEmoji} {Formatter.Bold(pickOrBan)} карты от капитана: {Formatter.Bold(currentTeam.Name)} {linkCapitan}");

            return builderButton;
        }

        public Builders.MessageBuilder CreateResultVoting(Models.Referee.StartPickBan data)
        {
            Builders.MessageBuilder builderButton;
            Builders.Embeds embeds = new Builders.Embeds()
                .Init()
                .AddDescription($"Голосование команд {Formatter.Bold(data.Team1Name)} и {Formatter.Bold(data.Team2Name)} по выбору карт завершено");

            List<string> mapsLeft = new(_allMaps);

            StringBuilder builderRestMap = new();
            int i = 1;
            foreach (Models.PickBans.PickBanDetailed itemPickBan in data.PickBanMap.PickBanDetailed)
            {
                if (itemPickBan.PickBanType == Models.PickBanType.pick)
                    builderRestMap.AppendLine($"{i++}: {itemPickBan.PickBanName}   {itemPickBan.Team?.Name}");

                if (!string.IsNullOrWhiteSpace(itemPickBan.PickBanName))
                    mapsLeft.Remove(Resources.DictionaryTemplates.GetKeyMap(itemPickBan.PickBanName));
            }

            string lastMap = Resources.DictionaryTemplates.GetMap(mapsLeft[0]);

            builderRestMap.AppendLine($"{i}. {lastMap}");

            if (data.Mode == Models.PickBanMode.bestOf1)
                embeds.AddField("Бой пройдёт на карте:", builderRestMap.ToString());
            else
                embeds.AddField("Бои пройдут на картах в следующей последовательности:", builderRestMap.ToString());

            builderRestMap.Clear();

            data.PickBanMap.DateEnd = DateTime.Now;
            data.PickBanMap.AddDetails(new Models.Teams.Team(" /--/ "), Models.PickBanType.none, lastMap);

            builderButton = new Builders.MessageBuilder()
                .AddEmbed(embeds.GetEmbed());

            return builderButton;
        }


        private bool AddButtonPickBanMessage(Models.PickBans.PickBan data,
                                             Builders.MessageBuilder builderButton)
        {
            bool detailedIdsChanged = false;

            foreach (var item in Enum.GetValues(typeof(Models.MapsTournamentHacking)))
            {
                string nameMap = item.ToString()!;

                if (!_allMaps.Contains(nameMap))
                    continue;

                string label = Resources.DictionaryTemplates.GetMap(nameMap);
                bool disabled = data.PickBanDetailed.Any(el => el.PickBanName == label);

                ButtonStyle currentButtonStyle = ButtonStyle.Primary;
                if (disabled)
                {
                    Models.PickBanType typeCurrentMap = data.PickBanDetailed.First(el => el.PickBanName == label).PickBanType;

                    if (typeCurrentMap == Models.PickBanType.pick)
                        currentButtonStyle = ButtonStyle.Success;
                    else if (typeCurrentMap == Models.PickBanType.ban)
                        currentButtonStyle = ButtonStyle.Danger;
                }

                if (data.GetButtonByLabel(label) == null)
                {
                    data.AddDetailedId(
                        label,
                        builderButton.AddButton(label, disabled, currentButtonStyle));

                    detailedIdsChanged = true;
                }
                else
                    builderButton.AddButton(label, disabled, currentButtonStyle, data.GetButtonByLabel(label)?.Id ?? string.Empty);
            }

            return detailedIdsChanged;
        }

    }
}
