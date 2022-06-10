namespace CaliberTournamentsV2.Models.PickBans
{
    public class PickBanDetailedId
    {
        public PickBanDetailedId(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }
}
