using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2
{
    internal class Logger
    {
        private Builders.MessageBuilder? _logBuilder = default;

        public Logger(Models.PickBans.PickBanMap? logsMaps)
        {
            LogsMaps = logsMaps;
        }

        Models.PickBans.PickBanMap? LogsMaps { get; } = default;

        internal void CreateBuilderLogs(string description, string author)
        {
            _logBuilder = new Builders.MessageBuilder();
                //.AddDescription(description);


            if (LogsMaps != default)
            {
                Builders.Embeds embedsLogMaps = new Builders.Embeds()
                    .Init(author)
                    .AddDescription(description);

                embedsLogMaps.AddField("Период", $"{LogsMaps.DateStart.GetFormattedTime()}-{LogsMaps.DateEnd.GetFormattedTime()}", true);
                embedsLogMaps.AddField("Пик", LogsMaps.GetFormatterDetailed(Models.PickBanType.pick), true);
                embedsLogMaps.AddField("Бан", LogsMaps.GetFormatterDetailed(Models.PickBanType.ban), true);

                _logBuilder.AddEmbed(embedsLogMaps.GetEmbed());


                foreach (Models.PickBans.PickBanDetailed itemMap in LogsMaps.PickBanDetailed)
                {
                    if (itemMap.Operators.ResultGenerated)
                    {

                        Builders.Embeds embedsLogOperators = new Builders.Embeds()
                            .Init($"{author}. Карта {Formatter.Bold(itemMap.PickBanName)}")
                            .AddDescription(description);

                        embedsLogOperators.AddField("Период", $"{itemMap.Operators.DateStart.GetFormattedTime()}-{itemMap.Operators.DateEnd.GetFormattedTime()}", true);
                        embedsLogOperators.AddField("Пик", itemMap.Operators.GetFormatterDetailed(Models.PickBanType.pick), true);
                        embedsLogOperators.AddField("Бан", itemMap.Operators.GetFormatterDetailed(Models.PickBanType.ban), true);

                        _logBuilder.AddEmbed(embedsLogOperators.GetEmbed());
                    }
                }
            }
        }

        internal void SendMessage(ulong idChannel, ulong idMessage)
        {
            if (_logBuilder == default)
                return;

            MessageQueue.Add(idChannel, _logBuilder.GetMessage(), idMessage);
        }
    }
}
