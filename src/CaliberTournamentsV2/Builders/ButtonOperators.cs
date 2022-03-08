using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Builders
{
    internal class ButtonOperators
    {
        private static readonly Dictionary<string, Array> _dictClassOperators;
        private static readonly Dictionary<ulong, List<string>> _cacheSendedButton;

        static ButtonOperators()
        {
            _dictClassOperators = new()
            {
                { "Штурмовик", Enum.GetValues(typeof(Models.OperatorsAssault)) },
                { "Поддержка", Enum.GetValues(typeof(Models.OperatorsSupport)) },
                { "Медик", Enum.GetValues(typeof(Models.OperatorsMedic)) },
                { "Снайпер", Enum.GetValues(typeof(Models.OperatorsMarksman)) }
            };

            _cacheSendedButton = new Dictionary<ulong, List<string>>();
        }
        public ButtonOperators(DiscordChannel channel,
                               Models.PickBans.PickBanDetailed detailedMap,
                               Models.PickBans.PickBanOperators data,
                               Models.Teams.Team? team,
                               Models.PickBanType pickBanType)
        {
            Channel = channel;
            DetailedOperators = detailedMap;
            Data = data;
            Team = team;
            PickBanType = pickBanType;
        }

        internal DiscordChannel Channel { get; }
        internal Models.PickBans.PickBanDetailed DetailedOperators { get; }
        internal Models.PickBans.PickBanOperators Data { get; }
        internal Models.Teams.Team? Team { get; }
        internal Models.PickBanType PickBanType { get; }

        internal async Task SendButtonByKeyClassOperators(string classOperator)
        {
            MessageBuilder builderButton = new MessageBuilder()
                .AddDescription(classOperator);

            bool detailedIdsChanged = AddButton(builderButton, classOperator);

            if (detailedIdsChanged)
            {
                bool sendMessage = true;

                string builderButtonHash = CompressCache($"{classOperator}{builderButton.GetCache()}");

                if (Data.IsEnded)
                    builderButtonHash = string.Empty;

                if (_cacheSendedButton.ContainsKey(Channel.Id))
                {
                    int countNonPickedOperators = Data.TeamOperators.Values.Count(el => el.Any(el2 => el.Any(el3 => el3.ClassOperator == classOperator && el3.OperatorName != null)));

                    if (_cacheSendedButton[Channel.Id].Any(el => el == builderButtonHash))
                    {
                        //if (countNonPickedOperators == 0 || countNonPickedOperators == 2)
                        if (countNonPickedOperators == 1)
                        {
                            _cacheSendedButton[Channel.Id].Remove(builderButtonHash);
                            //Worker.LogInf($"Удалён кэш. Канал: {Channel.Id}. Класс: {classOperator}. ID: {builderButtonHash}");
                            builderButtonHash = string.Empty;
                        }
                        else
                        {
                            //Worker.LogInf($"Найден кэш. Канал: {Channel.Id}. Класс: {classOperator}. ID: {builderButtonHash}");
                            sendMessage = false;
                        }
                    }
                }
                else
                    _cacheSendedButton.Add(Channel.Id, new List<string>());

                if (sendMessage)
                {
                    if (!string.IsNullOrEmpty(builderButtonHash))
                    {
                        //Worker.LogInf($"Создан кэш. Канал: {Channel.Id}. Класс: {classOperator}. ID: {builderButtonHash}");
                        _cacheSendedButton[Channel.Id].Add(builderButtonHash);
                    }

                    if (DetailedOperators.ClassOperatorsMessages.ContainsKey(classOperator))
                    {
                        MessageQueue.Add(Channel.Id, builderButton.GetMessage(), DetailedOperators.ClassOperatorsMessages[classOperator]);
                    }
                    else
                    {
                        DiscordMessageBuilder messageBuilder = builderButton.GetMessage();

                        try
                        {
                            DiscordMessage messageAssault = await Channel.SendMessageAsync(messageBuilder);
                            DetailedOperators.ClassOperatorsMessages.Add(classOperator, messageAssault.Id);
                        }
                        catch (Exception ex)
                        {
                            Worker.LogErr(ex.ToString());
                            Worker.LogWarn("Попытка отправки сообщения через очередь:");

#pragma warning disable IDE0039 // its ok
                            Action<ulong> action = (ulong messageId) => { DetailedOperators.ClassOperatorsMessages.Add(classOperator, messageId); };
#pragma warning restore IDE0039 // its ok

                            MessageQueue.Add(Channel.Id, messageBuilder, action);
                        }
                    }
                }
            }
        }

        private bool AddButton(MessageBuilder builderButton, string classOperator)
        {
            bool updateMessage = false;

            bool currentTeamSended = false;
            bool currentTeamVoted = false;
            bool opponentTeamSended = false;
            bool opponentTeamVoted = false;

            foreach (KeyValuePair<Models.Teams.Team, List<Models.PickBans.PickOperatorsData>> itemTeam in Data.TeamOperators)
            {
                if (itemTeam.Key == Team)
                {
                    currentTeamSended = itemTeam.Value.First(el => el.ClassOperator == classOperator).MessageSended;
                    currentTeamVoted = !string.IsNullOrEmpty(itemTeam.Value.First(el => el.ClassOperator == classOperator).OperatorName);
                }
                else
                {
                    opponentTeamSended = itemTeam.Value.First(el => el.ClassOperator == classOperator).MessageSended;
                    opponentTeamVoted = !string.IsNullOrEmpty(itemTeam.Value.First(el => el.ClassOperator == classOperator).OperatorName);
                }
            }

            Array enumOperators = _dictClassOperators[classOperator];
            try
            {
                foreach (object itemOperator in enumOperators)
                {
                    string label = Resources.DictionaryTemplates.GetOperators(itemOperator.ToString()!);
                    bool disabledCurrentButton = Data.PickBanDetailed.Any(el => el.PickBanName == label);

                    ButtonStyle currentButtonStyle = ButtonStyle.Primary;
                    if (disabledCurrentButton)
                    {
                        Models.PickBanType typePreviosTurn = Data.PickBanDetailed.Last(el => el.PickBanName == label).PickBanType;

                        if (typePreviosTurn == Models.PickBanType.pick)
                        {
                            currentButtonStyle = ButtonStyle.Success;

                            if (Team != null)
                            {
                                if (!Data.TeamOperators[Team].Any(el => el.OperatorName == label))
                                {
                                    disabledCurrentButton = false;
                                    updateMessage = true;
                                }
                                else if (PickBanType == Models.PickBanType.ban)
                                {
                                    disabledCurrentButton = false;
                                    updateMessage = true;
                                }
                            }
                        }
                        else if (typePreviosTurn == Models.PickBanType.ban)
                        {

                            bool findedPick = Data.PickBanDetailed.Where(el => el.PickBanName == label).Any(el => el.PickBanType == Models.PickBanType.pick);

                            if (findedPick)
                                currentButtonStyle = ButtonStyle.Secondary;
                            else
                                currentButtonStyle = ButtonStyle.Danger;
                        }
                    }

                    if (Data.IsEnded)
                    {
                        disabledCurrentButton = true;
                        updateMessage = true;
                    }
                    if (!currentTeamSended && !updateMessage)
                        updateMessage = true;

                    if (currentTeamVoted && !disabledCurrentButton && PickBanType != Models.PickBanType.ban)
                        disabledCurrentButton = true;

                    if (currentTeamVoted && opponentTeamVoted && !disabledCurrentButton)
                        disabledCurrentButton = true;

                    if (Data.GetButtonByLabel(label) == null)
                    {
                        Data.AddDetailedId(
                            label,
                            builderButton.AddButtonPrefix(label, disabledCurrentButton, currentButtonStyle, classOperator));

                        updateMessage = true;
                    }
                    else
                    {
                        builderButton.AddButton(label, disabledCurrentButton, currentButtonStyle, Data.GetButtonByLabel(label)?.Id ?? string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Worker.LogErr(ex.ToString());
            }

            return updateMessage;
        }

        private static string CompressCache(string value)
            => value.GetHashCode().ToString();
    }
}
