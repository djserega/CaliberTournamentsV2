using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;

namespace CaliberTournamentsV2.Models.PickBans
{
    internal abstract class PickBan
    {
        internal PickBan(PickBanKind kind)
        {
            Kind = kind;
        }

        internal bool IsActive { get => DateStart != default && DateEnd == default; }
        [JsonProperty]
        internal bool IsEnded { get => DateEnd != default; }
        [JsonProperty]
        internal DateTime DateStart { get; set; }
        [JsonProperty]
        internal DateTime DateEnd { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        internal PickBanKind Kind { get; set; }
        [JsonProperty]
        internal List<PickBanDetailed> PickBanDetailed { get; set; } = new List<PickBanDetailed>();
        internal List<PickBanDetailedId> DetailedIds { get; set; } = new List<PickBanDetailedId>();

        internal void AddDetailedId(string label, string id)
        {
            DetailedIds.Add(new(label, id));
        }

        internal void AddDetails(Teams.Team? command, PickBanType type, string name = "")
        {
            PickBanDetailed.Add(new PickBanDetailed()
            {
                Id = PickBanDetailed.Count,
                Team = command,
                PickBanType = type,
                PickBanName = name
            });
        }

        internal PickBanDetailedId? GetButtonByLabel(string? label)
        {
            if (DetailedIds.Any(el => el.Name == label))
                return DetailedIds.First(el => el.Name == label);

            return default;
        }

        internal string GetFormatterDetailed(PickBanType? type = null)
        {
            StringBuilder result = new();

            foreach (PickBanDetailed item in PickBanDetailed)
            {
                if (type == null)
                    AppendDetailedToResult(result, item);
                else if (item.PickBanType == type)
                    AppendDetailedToResult(result, item);
            }

            return result.ToString();
        }

        private static void AppendDetailedToResult(StringBuilder builder, PickBanDetailed detailed)
        {
            builder.Append(detailed?.Team?.Name);
            builder.Append(" - ");
            builder.AppendLine(detailed?.PickBanName);
        }


        internal string GetMainInfo(StringBuilder sb)
        {
            sb.Append("Время: ");
            sb.Append(DateStart.ToString("HH:mm:ss"));
            sb.Append(" - ");
            sb.AppendLine(DateEnd.ToString("HH:mm:ss"));

            foreach (PickBanDetailed itemDetailed in PickBanDetailed)
            {
                if (itemDetailed.Team != null)
                {
                    sb.Append("Id: ");
                    sb.Append(itemDetailed.Id.ToString());
                    sb.Append(" - ");
                    sb.Append(itemDetailed.Team?.Name);
                    sb.Append(" - ");
                    sb.Append(itemDetailed.PickBanName);
                    if (itemDetailed.CheckedDateTime != null)
                    {
                        sb.Append(" (");
                        sb.Append(itemDetailed.CheckedDateTime?.ToString("HH:mm:ss"));
                        sb.Append(')');
                    }
                    sb.AppendLine();
                }
            }

            string info = sb.ToString();
            sb.Clear();
            return info;
        }
    }
}
