using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2
{
    public partial class Worker
    {
        private static event EventHandler<string>? LogInfEvent;
        private static event EventHandler<string>? LogWarnEvent;
        private static event EventHandler<string>? LogErrEvent;

        public void InitWorkerEvents()
        {
#pragma warning disable CA2254 // Template should be a static expression
            LogInfEvent += (object? sender, string message) => _logger?.LogInformation(message);
            LogWarnEvent += (object? sender, string message) => _logger?.LogWarning(message);
            LogErrEvent += (object? sender, string message) => _logger?.LogError(message);
#pragma warning restore CA2254 // Template should be a static expression
        }

        internal static void LogInf(string message) => LogInfEvent?.Invoke(null, message);
        internal static void LogWarn(string message) => LogWarnEvent?.Invoke(null, message);
        internal static void LogErr(string message) => LogErrEvent?.Invoke(null, message);
    }
}
