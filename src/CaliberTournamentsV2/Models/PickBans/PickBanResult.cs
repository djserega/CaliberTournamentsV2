using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Models.PickBans
{
    public class PickBanResult
    {
        public PickBanResult()
        {
        }

        public PickBanResult(string text) : this()
        {
            Text = text;
        }

        public PickBanResult(bool result, string text) : this(text)
        {
            Result = result;
        }

        public bool Result { get; set; }
        public string? Text { get; set; }
        public string? Text2 { get; set; }
    }
}
