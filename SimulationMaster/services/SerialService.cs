using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Threading.Tasks;
using SharedConfig;
namespace SimulationMaster.services
{
    public class SerialService
    {
        private readonly SerialPort _serialPort;

        private readonly CsvService _csvService;

        public SerialService(CsvService csvService)
        {
            _serialPort = new SerialPort(SharedConfig.ConfigManager.Get("MASTER_PORT_NAME").Trim(), SharedConfig.ConfigManager.GetInt("BAUD_RATE"))
            {
                NewLine = "\n",
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };
            this._csvService = csvService;


        }

        private string GenerateAnchorId()
        {
            var rand = new Random();
            return rand.Next(4).ToString();
        }

        private string CreateRequest(string anchorId)
        {
            return $"{SharedConfig.ConfigManager.Get("GET_DISTANCE_COMMAND").Trim()}:{anchorId}";
        }

        private async Task Send()
        {
            Console.WriteLine("Sending time request to slave...");
            this._serialPort.WriteLine(this.CreateRequest(this.GenerateAnchorId()));


            await Task.Delay(TimeSpan.FromSeconds(5));  
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            try
            {
                string response = port.ReadLine();
                var parts = response.Split('|');
                this._csvService.Log(parts);

                Console.WriteLine($"[Master] Received: {response}");

            }
            catch (TimeoutException) { }
        }

        public async Task Start()
        {
            try
            {
                this._serialPort.Open();
                this._serialPort.DataReceived += OnDataReceived;
                while (true)
                {
                    await this.Send();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }
    }
}