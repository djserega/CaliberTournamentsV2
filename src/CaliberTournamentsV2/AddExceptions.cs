using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2
{
    internal class InitException : Exception
    {
        internal InitException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
