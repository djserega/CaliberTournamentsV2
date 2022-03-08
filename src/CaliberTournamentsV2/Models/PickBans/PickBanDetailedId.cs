using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
