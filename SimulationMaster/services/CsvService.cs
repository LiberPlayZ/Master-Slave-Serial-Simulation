using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedConfig;
namespace SimulationMaster.services
{
    public class CsvService
    {
        private readonly string _filePath;

        public CsvService()
        {
            this._filePath = SharedConfig.ConfigManager.Get("CSV_PATH").Trim();
            this.Initialize();
        }

        private void Initialize()
        {
            string folderPath = Path.GetDirectoryName(_filePath)!;


            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }


            if (!File.Exists(_filePath))
            {
                using (var writer = new StreamWriter(_filePath, false))
                {
                    writer.WriteLine("Timestamp,Time,Distance,Id\n");
                }
            }


        }

        // the function is adding the data to csv by new line . 
        public void Log(string response)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var line = $"{timestamp},{response}";
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }
}