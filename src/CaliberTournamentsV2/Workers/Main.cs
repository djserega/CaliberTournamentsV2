using System.Reflection;

namespace CaliberTournamentsV2
{
    public partial class Worker
    {
        private IConfigurationRoot? _config;

        public void InitWorkerMain()
        {
            InitBuilder();
        }

        private void InitBuilder()
        {
            try
            {
                Console.WriteLine("Initializing config-file...");

                _config = new ConfigurationBuilder()
                    .SetBasePath(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName)
                    .AddJsonFile("config.json", false, true)
                    .Build();
            }
            catch (Exception ex)
            {
                throw new InitException("Failed initializing config-file.", ex);
            }
        }

    }
}
