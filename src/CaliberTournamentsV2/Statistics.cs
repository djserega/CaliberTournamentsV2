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
            string message = type switch
            {
                Models.StatisticTypes.operators => GetStatOperators(details),
                Models.StatisticTypes.maps => ""
            };

            return message;
        }

        private string GetStatOperators(Models.StatisticDetailedTypes details)
        {
            List<Models.Referee.StartPickBan> listData = Models.Referee.StartPickBan.ListPickBans;

            IEnumerable<Models.PickBans.PickBanDetailed> filterData = listData
                .Select(el => el.PickBanMap)
                .SelectMany(el => el.PickBanDetailed
                    .Select(el2 => el2.Operators))
                .SelectMany(el => el.PickBanDetailed)
                .Where(el => el.PickBanType == Models.PickBanType.pick && el.Cheched);

            foreach (Models.PickBans.PickBanDetailed item in filterData)
                UpdateDictionary(item.PickBanName ?? string.Empty);

            var sortedData = _dict.OrderByDescending(el => el.Value);

            StringBuilder builderResult = new();
            builderResult.AppendLine("Оперативники:");
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
