using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Threading.Tasks;
using SimulationMaster.config;
namespace SimulationMaster.services
{
    public class SerialService
    {
        private readonly SerialPort _serialPort;
        private readonly string _getDistanceCommand;
        public SerialService(ConfigData config)
        {
            this._serialPort = new SerialPort(config.PortName.Trim(), config.BaudRate)
            {
                NewLine = "\n",
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };
            this._getDistanceCommand = config.GetDistanceCommand.Trim();

        }

        private string GenerateAnchorId()
        {
            var rand = new Random();
            return rand.Next(4).ToString();
        }

        private string CreateRequest(string anchorId)
        {
            return $"{this._getDistanceCommand}:{anchorId}";
        }

        private async Task Send()
        {
            Console.WriteLine("Sending time request to slave...");
            this._serialPort.WriteLine(this.CreateRequest(this.GenerateAnchorId()));


            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        private static void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            try
            {
                string response = port.ReadLine();
                var parts = response.Split('|');
                foreach (var part in parts)
                {
                    System.Console.WriteLine(part);
                }

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