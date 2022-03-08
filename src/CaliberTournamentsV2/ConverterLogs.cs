using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2
{
    internal class ConverterLogs
    {
        internal record DataLog
        {
            [JsonProperty]
            internal int Id;

            [JsonProperty]
            internal string? Referee;
            [JsonProperty]
            internal string? Team1;
            [JsonProperty]
            internal string? Team2;
            [JsonProperty]
            internal string? Capitan1;
            [JsonProperty]
            internal string? Capitan2;

            [JsonConverter(typeof(StringEnumConverter))]
            internal Models.PickBanMode Mode;

            [JsonProperty]
            internal string? PollingMapStart;
            [JsonProperty]
            internal string? PollingMapEnd;

            #region Maps

            [JsonProperty]
            internal Models.PickBanType Map1Result;
            [JsonProperty]
            internal string? Map1;
            [JsonProperty]
            internal Models.PickBanType Map2Result;
            [JsonProperty]
            internal string? Map2;
            [JsonProperty]
            internal Models.PickBanType Map3Result;
            [JsonProperty]
            internal string? Map3;
            [JsonProperty]
            internal Models.PickBanType Map4Result;
            [JsonProperty]
            internal string? Map4;
            [JsonProperty]
            internal Models.PickBanType Map5Result;
            [JsonProperty]
            internal string? Map5;

            #endregion

            #region Operators

            #region Map1

            [JsonProperty]
            internal string? Map1PollingOperatorStart;
            [JsonProperty]
            internal string? Map1PollingOperatorEnd;

            [JsonProperty]
            internal string? Map1Team1Assault;
            [JsonProperty]
            internal string? Map1Team1Support;
            [JsonProperty]
            internal string? Map1Team1Medic;
            [JsonProperty]
            internal string? Map1Team1Marksman;

            [JsonProperty]
            internal string? Map1Team2Assault;
            [JsonProperty]
            internal string? Map1Team2Support;
            [JsonProperty]
            internal string? Map1Team2Medic;
            [JsonProperty]
            internal string? Map1Team2Marksman;

            #endregion

            #region Map2

            [JsonProperty]
            internal string? Map2PollingOperatorStart;
            [JsonProperty]
            internal string? Map2PollingOperatorEnd;

            [JsonProperty]
            internal string? Map2Team1Assault;
            [JsonProperty]
            internal string? Map2Team1Support;
            [JsonProperty]
            internal string? Map2Team1Medic;
            [JsonProperty]
            internal string? Map2Team1Marksman;

            [JsonProperty]
            internal string? Map2Team2Assault;
            [JsonProperty]
            internal string? Map2Team2Support;
            [JsonProperty]
            internal string? Map2Team2Medic;
            [JsonProperty]
            internal string? Map2Team2Marksman;

            #endregion

            #region Map3

            [JsonProperty]
            internal string? Map3PollingOperatorStart;
            [JsonProperty]
            internal string? Map3PollingOperatorEnd;

            [JsonProperty]
            internal string? Map3Team1Assault;
            [JsonProperty]
            internal string? Map3Team1Support;
            [JsonProperty]
            internal string? Map3Team1Medic;
            [JsonProperty]
            internal string? Map3Team1Marksman;

            [JsonProperty]
            internal string? Map3Team2Assault;
            [JsonProperty]
            internal string? Map3Team2Support;
            [JsonProperty]
            internal string? Map3Team2Medic;
            [JsonProperty]
            internal string? Map3Team2Marksman;

            #endregion

            #region Map4

            [JsonProperty]
            internal string? Map4PollingOperatorStart;
            [JsonProperty]
            internal string? Map4PollingOperatorEnd;

            [JsonProperty]
            internal string? Map4Team1Assault;
            [JsonProperty]
            internal string? Map4Team1Support;
            [JsonProperty]
            internal string? Map4Team1Medic;
            [JsonProperty]
            internal string? Map4Team1Marksman;

            [JsonProperty]
            internal string? Map4Team2Assault;
            [JsonProperty]
            internal string? Map4Team2Support;
            [JsonProperty]
            internal string? Map4Team2Medic;
            [JsonProperty]
            internal string? Map4Team2Marksman;

            #endregion

            #region Map5

            [JsonProperty]
            internal string? Map5PollingOperatorStart;
            [JsonProperty]
            internal string? Map5PollingOperatorEnd;

            [JsonProperty]
            internal string? Map5Team1Assault;
            [JsonProperty]
            internal string? Map5Team1Support;
            [JsonProperty]
            internal string? Map5Team1Medic;
            [JsonProperty]
            internal string? Map5Team1Marksman;

            [JsonProperty]
            internal string? Map5Team2Assault;
            [JsonProperty]
            internal string? Map5Team2Support;
            [JsonProperty]
            internal string? Map5Team2Medic;
            [JsonProperty]
            internal string? Map5Team2Marksman;

            #endregion

            #endregion

        }

        internal static string Convert(List<Models.Referee.StartPickBan> listPickBans)
        {
            System.Reflection.TypeInfo typeLog = (System.Reflection.TypeInfo)typeof(DataLog);

            List<DataLog> dataLogs = new();

            foreach (Models.Referee.StartPickBan item in listPickBans)
            {
                DataLog log = new();

                log.Id = item.Id;
                log.Referee = item.NameReferee;
                log.Team1 = item.Team1Name;
                log.Team2 = item.Team2Name;
                log.Capitan1 = item.Team1?.Capitan?.Name;
                log.Capitan2 = item.Team2?.Capitan?.Name;
                log.Mode = item.Mode;

                Models.PickBans.PickBanMap itemMap = item.PickBanMap;

                log.PollingMapStart = itemMap.DateStart.GetFormattedTime();
                log.PollingMapEnd = itemMap.DateEnd.GetFormattedTime();

                for (int iMap = 1; iMap <= itemMap.PickBanDetailed.Count; iMap++)
                {
                    typeLog.GetDeclaredField($"Map{iMap}Result")?.SetValue(log, itemMap.PickBanDetailed[iMap - 1].PickBanType);
                    typeLog.GetDeclaredField($"Map{iMap}")?.SetValue(log, itemMap.PickBanDetailed[iMap - 1].PickBanName);

                    Models.PickBans.PickBanOperators itemOperators = itemMap.PickBanDetailed[iMap - 1].Operators;

                    typeLog.GetDeclaredField($"Map{iMap}PollingOperatorStart")?.SetValue(log, itemOperators.DateStart.GetFormattedTime());
                    typeLog.GetDeclaredField($"Map{iMap}PollingOperatorEnd")?.SetValue(log, itemOperators.DateEnd.GetFormattedTime());

                    int idTeam = 1;
                    foreach (KeyValuePair<Models.Teams.Team, List<Models.PickBans.PickOperatorsData>> itemOperator in itemOperators.TeamOperators)
                    {
                        string prefixFieldOperator = $"Map{iMap}Team{idTeam}";

                        typeLog.GetDeclaredField($"{prefixFieldOperator}Assault")?.SetValue(log, itemOperator.Value[0].OperatorName);
                        typeLog.GetDeclaredField($"{prefixFieldOperator}Support")?.SetValue(log, itemOperator.Value[1].OperatorName);
                        typeLog.GetDeclaredField($"{prefixFieldOperator}Medic")?.SetValue(log, itemOperator.Value[2].OperatorName);
                        typeLog.GetDeclaredField($"{prefixFieldOperator}Marksman")?.SetValue(log, itemOperator.Value[3].OperatorName);

                        idTeam++;

                        if (idTeam >= 2)
                            break;
                    }
                }

                dataLogs.Add(log);
            }

            return JsonConvert.SerializeObject(dataLogs);
        }

    }
}
