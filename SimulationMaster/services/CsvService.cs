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
        private readonly string _positionsFilePath;

        public CsvService()
        {
            this._logsPath = SharedConfig.ConfigManager.Get("LOGS_PATH").Trim();
            this._distanceFilePath = (this._logsPath + SharedConfig.ConfigManager.Get("DISTANCE_CSV_NAME")).Trim();
            this._positionsFilePath = (this._logsPath + SharedConfig.ConfigManager.Get("POSITIONS_CSV_NAME")).Trim();
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
                    writer.WriteLine("Timestamp,RequestId,Timer,Distance,Id\n");
                }
            }
            if (!File.Exists(this._positionsFilePath))
            {
                using (var writer = new StreamWriter(this._positionsFilePath, false))
                {
                    writer.WriteLine("Timestamp,RequestId,Timer,Type,Id,X,Y,Z\n");
                }
            }


        }

        // the function is adding the data to csv by new line . 
        public void LogDistance(string response)
        {
            var parts = response.Split(',');
            if (parts.Length < 5 || parts[0] != "DISTANCE")
            {
                System.Console.WriteLine("Unknown distance response");
                return;
            }
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var line = $"{timestamp},{parts[1]},{parts[2]},{parts[3]},{parts[4]}";
            File.AppendAllText(_distanceFilePath, line + Environment.NewLine);
        }
        public void LogPosition(string response)
        {
            var parts = response.Split(',');
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            if (parts.Length < 6)
            {
                System.Console.WriteLine("Unknown response");
                return;
            }
            if (parts[0] == "PILOT_POSITION")
            {
                var line = $"{timestamp},{parts[1]},{parts[2]},Pilot,,{parts[3]},{parts[4]},{parts[5]}";
                File.AppendAllText(_positionsFilePath, line + Environment.NewLine);
            }
            else if (parts[0] == "ANCHOR_POSITION")
            {
                if (parts.Length < 7)
                {
                    System.Console.WriteLine("Unknown response");
                    return;
                }
                var line = $"{timestamp},{parts[1]},{parts[2]},Anchor,{parts[3]},{parts[4]},{parts[5]},{parts[6]}";
                File.AppendAllText(_positionsFilePath, line + Environment.NewLine);
            }
            else
            {
                System.Console.WriteLine("Unknown response");
                return;
            }
        }

    }
}
