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

                LogInf("System startup...");

                _logger = logger;


                LogInf("Connection events...");

                InitWorkerEvents();


                LogInf("Base initialization...");

                InitWorkerMain();


                LogInf("Initialization data processor...");

                _dataHandler = new(_config);


                LogInf("Settings access...");

                _access = new(_config);


                LogInf("Startup discordbot...");

                _discordBot = new(_config);

            }
            catch (Exception ex)
            {
                LogErr("!!! Critical error at system startup !!!");
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