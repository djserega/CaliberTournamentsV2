using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2
{
    internal class Statistics
    {
        private Dictionary<string, int> _dict = new();

        internal string GetStatistics(Models.StatisticTypes type, Models.StatisticDetailedTypes details)
        {
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            string message = type switch
            {
                Models.StatisticTypes.operators => GetStatOperators(details),
                Models.StatisticTypes.maps => GetStatMaps(details),
                Models.StatisticTypes.all => throw new NotImplementedException()
            };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

            return message;
        }

        private string GetStatOperators(Models.StatisticDetailedTypes details)
        {
            List<Models.Referee.StartPickBan> listData = Models.Referee.StartPickBan.ListPickBans;

            IEnumerable<Models.PickBans.PickBanDetailed> listDetailedOperators = listData
                .Select(el => el.PickBanMap)
                .SelectMany(el => el.PickBanDetailed
                    .Select(el2 => el2.Operators))
                .SelectMany(el => el.PickBanDetailed);

            return GetStringStatistics(listDetailedOperators, details, "Оперативники:");
        }

        private string GetStatMaps(Models.StatisticDetailedTypes details)
        {
            List<Models.Referee.StartPickBan> listData = Models.Referee.StartPickBan.ListPickBans;

            IEnumerable<Models.PickBans.PickBanDetailed> listDetailsMaps = listData
                .Select(el => el.PickBanMap)
                .SelectMany(el => el.PickBanDetailed)
                    .Where(el => el.Cheched);

            return GetStringStatistics(listDetailsMaps, details, "Карты:");
        }

        private string GetStringStatistics(IEnumerable<Models.PickBans.PickBanDetailed> listSelectedData, Models.StatisticDetailedTypes details, string header)
        {

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            IEnumerable<Models.PickBans.PickBanDetailed> filterData = details switch
            {
                Models.StatisticDetailedTypes.picked => listSelectedData.Where(el => el.PickBanType == Models.PickBanType.pick && el.Cheched),
                Models.StatisticDetailedTypes.banned => listSelectedData.Where(el => el.PickBanType == Models.PickBanType.ban && el.Cheched),
                Models.StatisticDetailedTypes.none => listSelectedData.Where(el => el.Cheched),
            };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

            foreach (Models.PickBans.PickBanDetailed item in filterData)
                UpdateDictionary(item.PickBanName ?? string.Empty);

            var sortedData = _dict.OrderByDescending(el => el.Value);

            StringBuilder builderResult = new();
            builderResult.AppendLine(header);
            foreach (var item in sortedData)
                builderResult.AppendLine($"{item.Key} - {item.Value}");

            return builderResult.ToString();
        }

        private void UpdateDictionary(string key)
        {
            if (_dict.ContainsKey(key))
                _dict[key]++;
            else
                _dict.Add(key, 1);
        }
    }
}
