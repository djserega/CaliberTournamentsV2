namespace CaliberTournamentsV2
{
    public partial class Worker : BackgroundService
    {
        private readonly ILogger<Worker>? _logger;

#pragma warning disable IDE0052 // read private members
        private readonly Bot.DiscordBot? _discordBot;
        private readonly DataHandlers.DataHandler? _dataHandler;
        private readonly Access? _access;
#pragma warning restore IDE0052 // read private members

        public Worker(ILogger<Worker> logger)
        {
            try
            {

                LogInf("Запуск системы...");

                _logger = logger;


                LogInf("Подключение событий...");

                InitWorkerEvents();


                LogInf("Базовая инициализация...");

                InitWorkerMain();


                LogInf("Подготовка обработчика данных...");

                _dataHandler = new(_config);


                LogInf("Настройка доступов...");

                _access = new(_config);


                LogInf("Запуск discord-бота...");

                _discordBot = new(_config);

            }
            catch (Exception ex)
            {
                LogErr("!!! Критическая ошибка запуска приложения !!!");
                LogErr(ex.ToString());
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger?.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(600 * 1000, stoppingToken);
            }
        }
    }
}