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

        private int anchorId = 0;

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

        private string GetAnchorId()
        {
            var getAnchor = (this.anchorId % 3) + 1;
            return getAnchor.ToString();
        }

        private void IncreaseAnchorId()
        {
            this.anchorId++;
        }

        private string CreateRequest(string anchorId)
        {
            return $"{SharedConfig.ConfigManager.Get("GET_DISTANCE_COMMAND").Trim()}:{anchorId}";
        }

        private async Task Send()
        {
            Console.WriteLine("Sending time request to slave...");
            this._serialPort.WriteLine(this.CreateRequest(this.GetAnchorId()));


            await Task.Delay(TimeSpan.FromSeconds(5));  
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            try
            {
                string response = port.ReadLine();
                this._csvService.Log(response);

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
                    this.IncreaseAnchorId();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }
    }
}