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
                    writer.WriteLine("Timestamp,Time,Distance\n"); 
                }
            }


        }

        public void Log(string[] parts)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var lines = string.Join(Environment.NewLine, parts.Select(p => $"{timestamp},{p}"));
            File.AppendAllText(_filePath, lines + Environment.NewLine);
        }
    }
}