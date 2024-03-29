﻿using DSharpPlus.Entities;
using System.Text;

namespace CaliberTournamentsV2
{
    internal class Statistics
    {
        internal DiscordEmbed[] GetStatistics(Models.StatisticTypes type, Models.StatisticDetailedTypes details)
        {
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            DiscordEmbed[] embedMessage = type switch
            {
                Models.StatisticTypes.operators => GetStatOperators(details),
                Models.StatisticTypes.maps => GetStatMaps(details),
                Models.StatisticTypes.mapsAndOperators => GetMapsAndOperators(details),
                Models.StatisticTypes.all => throw new NotImplementedException()
            };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

            return embedMessage;
        }

        private DiscordEmbed[] GetStatOperators(Models.StatisticDetailedTypes details)
        {
            List<Models.Referee.StartPickBan> listData = Models.Referee.StartPickBan.ListPickBans;

            IEnumerable<Models.PickBans.PickBanDetailed> listDetailedOperators = listData
                .Select(el => el.PickBanMap)
                .SelectMany(el => el.PickBanDetailed
                    .Select(el2 => el2.Operators))
                .SelectMany(el => el.PickBanDetailed);

            return new[] { GetEmbedStatistics(listDetailedOperators, details, Models.StatisticTypes.operators, el => el.Cheched) };
        }

        private DiscordEmbed[] GetStatMaps(Models.StatisticDetailedTypes details)
        {
            List<Models.Referee.StartPickBan> listData = Models.Referee.StartPickBan.ListPickBans;

            IEnumerable<Models.PickBans.PickBanDetailed> listDetailsMaps = listData
                .Select(el => el.PickBanMap)
                .SelectMany(el => el.PickBanDetailed);

            return new[] { GetEmbedStatistics(listDetailsMaps, details, Models.StatisticTypes.maps, el => !string.IsNullOrEmpty(el.PickBanName)) };
        }

        private DiscordEmbed[] GetMapsAndOperators(Models.StatisticDetailedTypes details)
        {
            List<Models.Referee.StartPickBan> listData = Models.Referee.StartPickBan.ListPickBans;

            IEnumerable<Models.PickBans.PickBanDetailed> listDetailsMaps = listData
                .Select(el => el.PickBanMap)
                .SelectMany(el => el.PickBanDetailed);

            List<DiscordEmbed> embeds = new();

            foreach (Models.PickBans.PickBanDetailed itemMap in listDetailsMaps)
                embeds.Add(GetEmbedStatistics(itemMap.Operators.PickBanDetailed,
                                              details,
                                              Models.StatisticTypes.maps,
                                              el => !string.IsNullOrEmpty(el.PickBanName),
                                              $" по карте {itemMap.PickBanName}"));

            return embeds.ToArray();
        }

        private DiscordEmbed GetEmbedStatistics(IEnumerable<Models.PickBans.PickBanDetailed> listSelectedData,
                                                Models.StatisticDetailedTypes details,
                                                Models.StatisticTypes type,
                                                Func<Models.PickBans.PickBanDetailed, bool> predicateNoneType,
                                                string postfixTitle = "")
        {

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            IEnumerable<Models.PickBans.PickBanDetailed> filterData = details switch
            {
                Models.StatisticDetailedTypes.picked => listSelectedData.Where(el => el.PickBanType == Models.PickBanType.pick && el.Cheched),
                Models.StatisticDetailedTypes.banned => listSelectedData.Where(el => el.PickBanType == Models.PickBanType.ban && el.Cheched),
                Models.StatisticDetailedTypes.none => listSelectedData.Where(predicateNoneType),
            };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

            Dictionary<string, int> dictPicked = new();
            Dictionary<string, int> dictBanned = new();
            Dictionary<string, int> dictDecider = new();

            foreach (Models.PickBans.PickBanDetailed item in filterData)
            {
                if (item.PickBanType == Models.PickBanType.pick)
                    UpdateDictionary(dictPicked, item.PickBanName ?? string.Empty);
                else if (item.PickBanType == Models.PickBanType.ban)
                    UpdateDictionary(dictBanned, item.PickBanName ?? string.Empty);
                else if (item.PickBanType == Models.PickBanType.none)
                    UpdateDictionary(dictDecider, item.PickBanName ?? string.Empty);
            }

            Builders.Embeds embedsMessage = new Builders.Embeds()
                .Init()
                .AddTitle("Сводная информация" + postfixTitle);

            StringBuilder builderResult = new();

            if (details == Models.StatisticDetailedTypes.picked
                || details == Models.StatisticDetailedTypes.none)
            {
                FilledDataFromDictionary("Picked", embedsMessage, builderResult, dictPicked.OrderByDescending(el => el.Value));
            }

            if (details == Models.StatisticDetailedTypes.banned
                || details == Models.StatisticDetailedTypes.none)
            {
                FilledDataFromDictionary("Banned", embedsMessage, builderResult, dictBanned.OrderByDescending(el => el.Value));
            }

            if (details == Models.StatisticDetailedTypes.none
                || details == Models.StatisticDetailedTypes.none)
            {
                FilledDataFromDictionary("Decider", embedsMessage, builderResult, dictDecider.OrderByDescending(el => el.Value));
            }

            return embedsMessage.GetEmbed();
        }

        private static void FilledDataFromDictionary(string fieldName, Builders.Embeds embedsMessage, StringBuilder builderResult, IOrderedEnumerable<KeyValuePair<string, int>> sortedDataPicked)
        {
            foreach (KeyValuePair<string, int> item in sortedDataPicked)
                builderResult.AppendLine($"{item.Key} - {item.Value}");

            embedsMessage.AddField(fieldName, builderResult.ToString(), true);

            builderResult.Clear();
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
