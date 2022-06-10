namespace CaliberTournamentsV2.Models.PickBans
{
    internal class PickBanMap : PickBan
    {
        internal PickBanMap() : base(PickBanKind.map)
        {
        }

        internal bool ResultGenerated { get; set; }
    }
}
