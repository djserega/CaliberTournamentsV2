using DSharpPlus.CommandsNext;

namespace CaliberTournamentsV2.Bot
{
    internal partial class DiscordBot
    {
        private CommandsNextExtension? _commands;

        internal void InitCommands(IConfigurationRoot? _config)
        {
            try
            {
                Console.WriteLine("Loading commands...");

                Console.WriteLine("Prefix:" + _config.GetValue<string>("discord:commandPrefix"));

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
                    Console.WriteLine($" -- loading -> {typeCommand.Name}.");

                    _commands.RegisterCommands(typeCommand);

                    Console.WriteLine($" -- loaded -> {typeCommand.Name}.");
                }

                Console.WriteLine($" Loaded modules: {typeList.Length}.");
            }
            catch (Exception ex)
            {
                throw new InitException("Error. Not initialized command modules", ex);
            }
        }
    }
}
