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

  

        private string CreateGetDistanceRequest(string anchorId)
        {
            return $"{SharedConfig.SerialCommand.GET_DISTANCE.ToWireString()}:{anchorId}";
        }

        private string CreateGetPilotPositionRequest()
        {
            return SharedConfig.SerialCommand.GET_PILOT_POSITION.ToWireString();

        }

        private string CreateGetAnchorPositionRequest(string anchorId)
        {
            return $"{SerialCommand.GET_ANCHOR_POSITION.ToWireString()}:{anchorId}";

        }



        private string? BuildRequestFromInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();

            switch (command)
            {
                case "distance":
                    if (parts.Length < 2) return null;
                    return this.CreateGetDistanceRequest(parts[1]);

                case "pilot":
                    return this.CreateGetPilotPositionRequest();

                case "anchor":
                    if (parts.Length < 2) return null;
                    return this.CreateGetAnchorPositionRequest(parts[1]);

                case "help":
                    return "HELP";

                default:
                    return null;
            }
        }



        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            try
            {
                string response = port.ReadLine();
                if (response.StartsWith("PILOT_POSITION,") || response.StartsWith("ANCHOR_POSITION,"))
                {
                    _csvService.LogPosition(response);
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
                Console.WriteLine("Enter commands: distance <id>, pilot, anchor <id>, help, exit");
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input == null) continue;

                    if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                        break;

                    var request = BuildRequestFromInput(input);

                    if (request == "HELP")
                    {
                        Console.WriteLine("Commands: distance <id>, pilot, anchor <id>, exit");
                        continue;
                    }

                    if (request == null)
                    {
                        Console.WriteLine("Unknown command. Try: distance <id>, pilot, anchor <id>");
                        continue;
                    }

                    _serialPort.WriteLine(request);
                }

                await Task.Delay(TimeSpan.FromSeconds(5));



            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }
    }
}
