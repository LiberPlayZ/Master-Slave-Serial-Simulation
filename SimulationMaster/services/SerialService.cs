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

        private int anchorsLen = 3;

        private bool cycle = false;

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
            return this.anchorsLen.ToString();
        }



        private string CreateGetDistanceRequest(string anchorId)
        {
            return $"{SharedConfig.SerialCommand.GET_DISTANCE.ToWireString()}:{anchorId}";
        }

        private string CreateGetPilotPositionRequest()
        {
            return SharedConfig.SerialCommand.GET_PILOT_POSITION.ToWireString();

        }


        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            try
            {
                string response = port.ReadLine();
                if (response.StartsWith("PILOT_POSITION:"))
                {
                    _csvService.LogPilotPosition(response);
                }
                else
                {
                    this._csvService.LogDistance(response);
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
                string req = "";
                while (true)
                {

                    if (this.cycle)
                    {
                        Console.WriteLine("Sending pilot request to slave...");
                        req = this.CreateGetPilotPositionRequest();
                        this.cycle = false;
                    }
                    else
                    {
                        this.anchorsLen = (this.anchorsLen % 3) + 1;
                        Console.WriteLine("Sending distance request to slave...");
                        req = this.CreateGetDistanceRequest(this.GetAnchorId());
                        if (this.anchorsLen == 3)
                        {
                            this.cycle = true;

                        }
                    }

                    this._serialPort.WriteLine(req);
                    await Task.Delay(TimeSpan.FromSeconds(5));

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }
    }
}