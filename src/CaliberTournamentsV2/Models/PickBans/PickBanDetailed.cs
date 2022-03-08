using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Models.PickBans
{
    internal class PickBanDetailed
    {
        private bool cheched;

        internal int Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        internal PickBanType PickBanType { get; set; }
        [JsonProperty]
        internal string? PickBanName { get; set; }
        [JsonProperty]
        internal Teams.Team? Team { get; set; }
        [JsonProperty]
        internal bool Cheched { get => cheched; set { cheched = value; CheckedDateTime = DateTime.Now; } }
        [JsonProperty]
        internal DateTime? CheckedDateTime { get; set; }

        internal Dictionary<string, ulong> ClassOperatorsMessages { get; set; } = new();
        [JsonProperty]
        internal PickBanOperators Operators { get; set; } = new();
        [JsonProperty]
        internal bool PickBanOperatorsIsEnded { get => Operators.IsEnded; }
    }
}
