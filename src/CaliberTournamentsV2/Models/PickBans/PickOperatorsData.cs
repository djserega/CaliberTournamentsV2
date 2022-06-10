using Newtonsoft.Json;

namespace CaliberTournamentsV2.Models.PickBans
{
    internal class PickOperatorsData
    {
        public PickOperatorsData(string classOperator)
        {
            ClassOperator = classOperator;
        }

        [JsonProperty]
        internal string ClassOperator { get; set; }
        [JsonProperty]
        internal string? OperatorName { get; set; }
        [JsonProperty]
        internal DateTime PickTime { get; set; }
        internal bool MessageSended { get; set; }
    }
}
