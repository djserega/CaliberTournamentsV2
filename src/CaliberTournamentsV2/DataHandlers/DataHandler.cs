using DSharpPlus.Entities;
using System.Text;

namespace CaliberTournamentsV2.DataHandlers
{
    internal class DataHandler
    {
        private static string[] _allMaps = Array.Empty<string>();
        private static readonly GoogleApis.Spreadsheets _spreadsheetsSaver;
        private static ulong _idChannelLogs;

        static DataHandler()
        {
            _spreadsheetsSaver = new GoogleApis.Spreadsheets();
        }

        internal DataHandler(IConfigurationRoot? config)
        {
            try
            {
                _allMaps = config?.GetSection("maps").GetStrings() ?? throw new NullReferenceException("Not found key list access map");

                if (_allMaps.Length == 0)
                    Worker.LogWarn("Not found/filled list access map");
                else
                {
                    Array addedMapsToEnums = Enum.GetValues(typeof(Models.MapsTournamentHacking));

                    List<string> enumMapsleft = new();
                    for (int i = 0; i < addedMapsToEnums.Length; i++)
                        enumMapsleft.Add(addedMapsToEnums.GetValue(i)!.ToString() ?? string.Empty);

                    List<string> mapsLeft = new(_allMaps);

                    foreach (string selectedMap in _allMaps)
                    {
                        if (enumMapsleft.Contains(selectedMap))
                        {
                            enumMapsleft.Remove(selectedMap);

                            if (mapsLeft.Contains(selectedMap))
                                mapsLeft.Remove(selectedMap);
                        }
                    }

                    if (mapsLeft.Count > 0)
                    {
                        StringBuilder stringBuilderErrorMessage = new("Ошибка в определении списка доступных карт:\n");
                        stringBuilderErrorMessage.Append("Осталось карт с ошибками: ");
                        stringBuilderErrorMessage.AppendLine(string.Join(", ", mapsLeft));

                        stringBuilderErrorMessage.Append("Осталось доступных карт: ");
                        stringBuilderErrorMessage.AppendLine(string.Join(", ", enumMapsleft));

                        throw new Exception(stringBuilderErrorMessage.ToString());
                    }
                }

                _idChannelLogs = config?.GetValue<ulong>("id_channel_log") ?? default;
            }
            catch (Exception ex)
            {
                throw new InitException("Не удалось загрузить параметры обработки данных", ex);
            }
        }

        internal static string[] GetAllMaps() => _allMaps;

        internal async static void ReceivedPickBanMessagesEvent(DiscordChannel channel,
                                                                DiscordMessage message,
                                                                ulong userId,
                                                                string buttonId)
        {
            bool processedMap = false;
            bool processedOperators = false;

            Models.Referee.StartPickBan? pickBanMaps = default;
            Models.PickBans.PickBanDetailed? detailedMap = default;

            SendPickBans.SendPickBanMap mapsHandler = new(_allMaps, channel, message);
            if (mapsHandler.ContainsButton(userId,
                                           buttonId,
                                           out pickBanMaps))
            {
                if (pickBanMaps != null)
                    mapsHandler.SendPickBanMessage(pickBanMaps);

                processedMap = true;
            }

            if (!processedMap)
            {
                Models.Referee.StartPickBan? pickBanMapForOperators = default;

                SendPickBans.SendPickBanOperators? operatorsHandler = new(channel);
                if (operatorsHandler.ContainsButton(userId,
                                                    buttonId,
                                                    out pickBanMapForOperators,
                                                    out Models.Referee.StartPickBan? startPickBanOperators,
                                                    out detailedMap))
                {
                    if (startPickBanOperators != null
                        && detailedMap != null)
                    {
                        operatorsHandler.SendPickBanMessage(startPickBanOperators,
                                                            detailedMap);

                        processedOperators = true;

                        if (pickBanMaps == default)
                            pickBanMaps = pickBanMapForOperators;
                    }
                }
            }

            if (processedMap || processedOperators)
            {
                if (processedMap && (pickBanMaps?.PickBanMap.ResultGenerated ?? false)
                    || (detailedMap?.Operators?.ResultGenerated ?? false)
                    )
                {
                    Logger logger = new(pickBanMaps?.PickBanMap);

                    logger.CreateBuilderLogs(pickBanMaps?.NameReferee ?? string.Empty,
                                             $"{pickBanMaps?.Team1Name} - {pickBanMaps?.Team2Name}");

                    if (pickBanMaps != null)
                    {
                        if (pickBanMaps.IdMessageLog == default)
                            pickBanMaps.IdMessageLog = await Bot.DiscordBot.SendMessage(_idChannelLogs, "...");

                        try
                        {
                            logger.SendMessage(_idChannelLogs, pickBanMaps.IdMessageLog);
                        }
                        catch (Exception ex)
                        {
                            Worker.LogErr($"Error sending log:\n{ex}");
                        }
                    }
                }

                await Task.Run(() =>
                {
                    UnloadData();
                });
            }
        }

        private static async void UnloadData()
        {
            try
            {
                await _spreadsheetsSaver.SaveDataListPickBans(Models.Referee.StartPickBan.ListPickBans);
            }
            catch (Exception ex)
            {
                Worker.LogErr(ex.ToString());
            }
        }

    }
}