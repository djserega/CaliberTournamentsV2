using Newtonsoft.Json;

namespace CaliberTournamentsV2.Models.PickBans
{
    internal class PickBanOperators : PickBan
    {
        internal bool ResultGenerated { get; set; }

        internal PickBanOperators() : base(PickBanKind.operators)
        {
            TeamOperators = new();
        }

        internal Dictionary<Teams.Team, List<PickOperatorsData>> TeamOperators { get; set; }

        internal record TeamOperatorsRecord 
        {
            [JsonProperty]
            internal Teams.Team? Team;
            [JsonProperty]
            internal List<PickOperatorsData>? Data; 
        };

        [JsonProperty]
        internal List<TeamOperatorsRecord> TeamOperatorsJson
        {
            get
            {
                List<TeamOperatorsRecord> teams = new();
                foreach (KeyValuePair<Teams.Team, List<PickOperatorsData>> keyValue in TeamOperators)
                {
                    teams.Add(new()
                    {
                        Team = keyValue.Key,
                        Data = keyValue.Value
                    });
                }
                return teams;
            }
        }

        internal void RegPick(Teams.Team team, string classOperator, string oper)
        {
            PickOperatorsData data = TeamOperators[team].First(el => el.ClassOperator == classOperator);

            data.OperatorName = oper;
            data.PickTime = DateTime.Now;
        }
    }
}
