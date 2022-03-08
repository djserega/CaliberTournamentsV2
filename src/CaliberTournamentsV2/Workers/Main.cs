using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                Console.WriteLine("Инициализация config-файла...");

                _config = new ConfigurationBuilder()
                    .SetBasePath(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName)
                    .AddJsonFile("config.json", false, true)
                    .Build();
            }
            catch (Exception ex)
            {
                throw new InitException("Не удалось инициализировать config-файл.", ex);
            }
        }

    }
}
