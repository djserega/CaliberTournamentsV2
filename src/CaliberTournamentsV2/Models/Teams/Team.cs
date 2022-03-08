using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Models.Teams
{
    internal class Team
    {
        public Team(string name)
        {
            Name = name;
        }
        public Team(string name, Player capitan) : this(name)
        {
            Capitan = capitan;
        }

        internal static List<Team> Teams { get; set; } = new();

        internal int Id { get; private set; }
        [JsonProperty]
        internal string Name { get; set; }
        [JsonProperty]
        internal Player? Capitan { get; set; }
        internal List<Player>? Players { get; set; }

        [JsonProperty]
        internal string LinkCapitan { get => Capitan?.UserId.GetLink() ?? string.Empty; }

        internal static void AddTeam(Team team)
        {
            team.Id = Teams.Count + 1;

            Teams.Add(team);
        }

        internal static bool TeamIsRegistered(string name)
            => TeamIsRegistered(el => el.Name == name);
        internal static bool TeamIsRegistered(Func<Team, bool> predicate)
              => Teams.Any(predicate);

        internal static bool CapitanIsRegistered(ulong id)
            => CapitanIsRegistered(el => el.Capitan?.UserId == id);
        internal static bool CapitanIsRegistered(Func<Team, bool> predicate)
            => Teams.Any(predicate);

        internal static Team? GetCommand(string name)
        {
            if (TeamIsRegistered(name))
                return Teams.First(el => el.Name == name);
            else
                return default;
        }
        internal static Team? GetCommand(Func<Team, bool> predicate)
        {
            if (TeamIsRegistered(predicate))
                return Teams.First(predicate);
            else
                return default;
        }

        internal string GetInfo()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.Append(Name);
            stringBuilder.Append(" : ");
            if (Players != null)
                stringBuilder.Append(string.Join(", ", Players.Select(el => el.Name)));

            return stringBuilder.ToString();
        }
    }
}
