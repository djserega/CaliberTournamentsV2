using DSharpPlus.Entities;
using System.Text;

namespace CaliberTournamentsV2
{
    internal class Statistics
    {
        internal DiscordEmbed GetStatistics(Models.StatisticTypes type, Models.StatisticDetailedTypes details)
        {
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            DiscordEmbed embedMessage = type switch
            {
                Models.StatisticTypes.operators => GetStatOperators(details),
                Models.StatisticTypes.maps => GetStatMaps(details),
                Models.StatisticTypes.all => throw new NotImplementedException()
            };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

            return embedMessage;
        }

        private DiscordEmbed GetStatOperators(Models.StatisticDetailedTypes details)
        {
            List<Models.Referee.StartPickBan> listData = Models.Referee.StartPickBan.ListPickBans;

            IEnumerable<Models.PickBans.PickBanDetailed> listDetailedOperators = listData
                .Select(el => el.PickBanMap)
                .SelectMany(el => el.PickBanDetailed
                    .Select(el2 => el2.Operators))
                .SelectMany(el => el.PickBanDetailed);

            return GetEmbedStatistics(listDetailedOperators, details, Models.StatisticTypes.operators);
        }

        private DiscordEmbed GetStatMaps(Models.StatisticDetailedTypes details)
        {
            List<Models.Referee.StartPickBan> listData = Models.Referee.StartPickBan.ListPickBans;

            IEnumerable<Models.PickBans.PickBanDetailed> listDetailsMaps = listData
                .Select(el => el.PickBanMap)
                .SelectMany(el => el.PickBanDetailed)
                    .Where(el => el.Cheched);

            return GetEmbedStatistics(listDetailsMaps, details, Models.StatisticTypes.maps);
        }

        private DiscordEmbed GetEmbedStatistics(IEnumerable<Models.PickBans.PickBanDetailed> listSelectedData, Models.StatisticDetailedTypes details, Models.StatisticTypes type)
        {

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            IEnumerable<Models.PickBans.PickBanDetailed> filterData = details switch
            {
                Models.StatisticDetailedTypes.picked => listSelectedData.Where(el => el.PickBanType == Models.PickBanType.pick && el.Cheched),
                Models.StatisticDetailedTypes.banned => listSelectedData.Where(el => el.PickBanType == Models.PickBanType.ban && el.Cheched),
                Models.StatisticDetailedTypes.none => listSelectedData.Where(el => el.Cheched),
            };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

            Dictionary<string, int> dictPicked = new();
            Dictionary<string, int> dictBanned = new();

            foreach (Models.PickBans.PickBanDetailed item in filterData)
            {
                if (item.PickBanType == Models.PickBanType.pick)
                    UpdateDictionary(dictPicked, item.PickBanName ?? string.Empty);
                else if (item.PickBanType == Models.PickBanType.ban)
                    UpdateDictionary(dictBanned, item.PickBanName ?? string.Empty);
            }

            Builders.Embeds embedsMessage = new Builders.Embeds()
                .Init()
                .AddTitle("Сводная информация");

            StringBuilder builderResult = new();

            if (details == Models.StatisticDetailedTypes.picked
                || details == Models.StatisticDetailedTypes.none)
            {
                IOrderedEnumerable<KeyValuePair<string, int>> sortedDataPicked = dictPicked.OrderByDescending(el => el.Value);

                foreach (KeyValuePair<string, int> item in sortedDataPicked)
                    builderResult.AppendLine($"{item.Key} - {item.Value}");

                embedsMessage.AddField("Picked", builderResult.ToString(), true);
                
                builderResult.Clear();
            }

            if (details == Models.StatisticDetailedTypes.banned
                || details == Models.StatisticDetailedTypes.none)
            {
                IOrderedEnumerable<KeyValuePair<string, int>> sortedDataBanned = dictBanned.OrderByDescending(el => el.Value);

                foreach (KeyValuePair<string, int> item in sortedDataBanned)
                    builderResult.AppendLine($"{item.Key} - {item.Value}");

                embedsMessage.AddField("Banned", builderResult.ToString(), true);

                builderResult.Clear();
            }

            return embedsMessage.GetEmbed();
        }

        private static void UpdateDictionary(Dictionary<string, int> dict, string key)
        {
            if (dict.ContainsKey(key))
                dict[key]++;
            else
                dict.Add(key, 1);
        }
    }
}
