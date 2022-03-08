using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaliberTournamentsV2.Bot
{
    internal partial class DiscordBot
    {
        private CommandsNextExtension? _commands;

        internal void InitCommands(IConfigurationRoot? _config)
        {
            try
            {
                Console.WriteLine("Загрузка комманд...");

                Console.WriteLine("Префикс:" + _config.GetValue<string>("discord:commandPrefix"));

                _commands = _discord.UseCommandsNext(new CommandsNextConfiguration()
                {
                    StringPrefixes = new[] { _config.GetValue<string>("discord:commandPrefix") },
                    EnableDefaultHelp = false
                });

                Type baseInterfaceCommand = typeof(Commands.ICommands);
                IEnumerable<Type> typesCommand = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => baseInterfaceCommand.IsAssignableFrom(p) && !p.IsInterface);

                Type[] typeList = typesCommand as Type[] ?? typesCommand.ToArray();
                foreach (Type typeCommand in typeList)
                {
                    Console.WriteLine($" -- загрузка -> {typeCommand.Name}.");

                    _commands.RegisterCommands(typeCommand);

                    Console.WriteLine($" -- загружено -> {typeCommand.Name}.");
                }

                Console.WriteLine($" Загружено модулей: {typeList.Length}.");
            }
            catch (Exception ex)
            {
                throw new InitException("Не удалось инициализировать команды", ex);
            }
        }
    }
}
