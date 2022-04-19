using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Models.Referee
{
    internal class StartPickBan
    {
        internal static List<StartPickBan> ListPickBans { get; set; } = new();

        internal StartPickBan(ulong channelId, DSharpPlus.Entities.DiscordUser referee, string command1, string command2)
        {
            ChannelId = channelId;
            IdReferee = referee.Id;
            NameReferee = referee.Username;
            Team1Name = command1;
            Team2Name = command2;

            Team1 = Teams.Team.GetCommand(command1);
            Team2 = Teams.Team.GetCommand(command2);
        }
        internal StartPickBan(ulong channelId, DSharpPlus.Entities.DiscordUser referee, string command1, string command2, string mode) : this(channelId, referee, command1, command2)
        {
            Mode = mode.ToLower() switch
            {
                "bo1" => PickBanMode.bestOf1,
                "bo3" => PickBanMode.bestOf3,
                "bo5" => PickBanMode.bestOf5,
                _ => PickBanMode.bestOf3,
            };
        }

        #region Properties

        [JsonProperty]
        internal int Id { get; private set; }

        [JsonProperty]
        internal ulong ChannelId { get; set; }
        [JsonProperty]
        internal ulong IdReferee { get; set; }
        [JsonProperty]
        internal string NameReferee { get; set; }

        [JsonProperty]
        internal PickBanMode Mode { get; set; }

        [JsonProperty]
        [NotNull]
        internal string Team1Name { get; set; }
        [JsonProperty]
        [NotNull]
        internal string Team2Name { get; set; }

        [JsonProperty]
        internal Teams.Team? Team1 { get; set; }
        [JsonProperty]
        internal Teams.Team? Team2 { get; set; }

        [JsonProperty]
        internal PickBans.PickBanMap PickBanMap { get; set; } = new();

        internal ulong IdMessageLog { get; set; } = default;

        #endregion

        internal static void AddPickBan(StartPickBan newPickBan)
        {
            newPickBan.Id = ListPickBans.Count + 1;

            ListPickBans.Add(newPickBan);
        }

        internal static StartPickBan? GetPickBan(string team1, string team2)
        {
            if (ListPickBans.Any(el => el.Team1Name == team1 && el.Team2Name == team2))
                return ListPickBans.Last(el => el.Team1Name == team1 && el.Team2Name == team2);
            if (ListPickBans.Any(el => el.Team2Name == team1 && el.Team1Name == team2))
                return ListPickBans.Last(el => el.Team2Name == team1 && el.Team1Name == team2);
            else
                return default;
        }

        internal void FillPickBansMap()
        {
            switch (Mode)
            {
                case PickBanMode.bestOf1:
                    PickBanMap.AddDetails(Team1, PickBanType.ban);
                    PickBanMap.AddDetails(Team2, PickBanType.ban);
                    PickBanMap.AddDetails(Team2, PickBanType.ban);
                    PickBanMap.AddDetails(Team1, PickBanType.ban);
                    break;
                case PickBanMode.bestOf3:
                    PickBanMap.AddDetails(Team1, PickBanType.ban);
                    PickBanMap.AddDetails(Team2, PickBanType.ban);
                    PickBanMap.AddDetails(Team2, PickBanType.pick);
                    PickBanMap.AddDetails(Team1, PickBanType.pick);
                    break;
                case PickBanMode.bestOf5:
                    PickBanMap.AddDetails(Team1, PickBanType.pick);
                    PickBanMap.AddDetails(Team2, PickBanType.pick);
                    PickBanMap.AddDetails(Team2, PickBanType.pick);
                    PickBanMap.AddDetails(Team1, PickBanType.pick);
                    break;
            }
        }

        internal string? FillPickBanOperators(Teams.Team? team1, Teams.Team? team2)
        {
            if (team1 == null || team2 == null)
            {
                return $"Не удалось найти команду {team1} и/или команду {team2}";
            }

            Func<PickBans.PickBanDetailed, bool> precidate = new(el =>
                (el.Cheched && el.PickBanType == PickBanType.pick || el.PickBanType == PickBanType.none)
                && !el.Operators.PickBanDetailed.Any());

            if (PickBanMap.PickBanDetailed.Any(precidate))
            {
                AddDetails(PickBanMap.PickBanDetailed.First(precidate).Operators, team1, team2);
            }

            return default;
        }

        private static void AddDetails(PickBans.PickBanOperators operators, Teams.Team team1, Teams.Team team2)
        {
            operators.TeamOperators.Add(
                team1,
                new List<PickBans.PickOperatorsData>()
                {
                    new ("Штурмовик"  ),
                    new ("Поддержка"   ),
                    new ("Медик"      ),
                    new ("Снайпер"    )
                });
            operators.TeamOperators.Add(
                team2,
                new List<PickBans.PickOperatorsData>()
                {
                    new ( "Штурмовик" ),
                    new ( "Поддержка"  ),
                    new ( "Медик"     ),
                    new ( "Снайпер"   )
                });

            operators.AddDetails(team1, PickBanType.ban);
            operators.AddDetails(team2, PickBanType.ban);
            operators.AddDetails(team1, PickBanType.pick);
            operators.AddDetails(team2, PickBanType.pick);
            operators.AddDetails(team1, PickBanType.pick);
            operators.AddDetails(team2, PickBanType.pick);

            operators.AddDetails(team2, PickBanType.ban);
            operators.AddDetails(team1, PickBanType.ban);
            operators.AddDetails(team2, PickBanType.pick);
            operators.AddDetails(team1, PickBanType.pick);
            operators.AddDetails(team2, PickBanType.pick);
            operators.AddDetails(team1, PickBanType.pick);
        }
    }
}
