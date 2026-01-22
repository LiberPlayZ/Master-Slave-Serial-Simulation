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
        private readonly string _logsPath;
        private readonly string _distanceFilePath;
        private readonly string _pilotFilePath;

        public CsvService()
        {
            this._logsPath = SharedConfig.ConfigManager.Get("LOGS_PATH").Trim();
            this._distanceFilePath = (this._logsPath + SharedConfig.ConfigManager.Get("DISTANCE_CSV_NAME")).Trim();
            this._pilotFilePath = (this._logsPath + SharedConfig.ConfigManager.Get("PILOT_CSV_NAME")).Trim();
            this.Initialize();
        }

        private void Initialize()
        {
            var folderPath = _logsPath;


            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(this._distanceFilePath))
            {
                using (var writer = new StreamWriter(this._distanceFilePath, false))
                {
                    writer.WriteLine("Timestamp,Time,Distance,Id\n");
                }
            }
            if (!File.Exists(this._pilotFilePath))
            {
                using (var writer = new StreamWriter(this._pilotFilePath, false))
                {
                    writer.WriteLine("Timestamp,X,Y,Z\n");
                }
            }


        }

        // the function is adding the data to csv by new line . 
        public void LogDistance(string response)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var line = $"{timestamp},{response}";
            File.AppendAllText(this._distanceFilePath, line + Environment.NewLine);
        }
        public void LogPilotPosition(string response)
        {

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var data = response.Replace("PILOT_POSITION:", "");
            File.AppendAllText(_pilotFilePath, $"{timestamp},{data}{Environment.NewLine}");
        }

    }
}