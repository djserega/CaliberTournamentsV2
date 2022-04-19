using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
