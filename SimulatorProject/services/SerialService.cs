using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using SharedConfig;
using SimulatorProject.enums;

namespace SimulatorProject.services
{
    public class SerialService
    {
        private readonly SerialPort _serialPort;
        private readonly TimerService _timer;

        private readonly SimulationService _simulationService;


        public SerialService(TimerService timer, SimulationService simulationService)
        {
            _timer = timer;
            this._simulationService = simulationService;
            _serialPort = new SerialPort(SharedConfig.ConfigManager.Get("SLAVE_PORT_NAME").Trim(), SharedConfig.ConfigManager.GetInt("BAUD_RATE"))
            {
                NewLine = "\n",
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };

        }



        public void Start()
        {
            try
            {
                _timer.Start();
                _serialPort.Open();
                _serialPort.DataReceived += async (sender, e) =>
                {
                    var sp = (SerialPort)sender;
                    try
                    {
                        string request = _serialPort.ReadLine().Trim();
                        Console.WriteLine($"Slave received: {request}");
                        string response = "";
                        string time = _timer.GetElapsedTime();
                        if (!request.StartsWith("REQ,"))
                        {
                            sp.WriteLine("BAD_REQUEST");
                            return;
                        }
                        string[] reqParts = request.Split(',', 4);
                        if (reqParts.Length < 3)
                        {
                            sp.WriteLine("BAD_REQUEST");
                            return;
                        }
                        string requestId = reqParts[1];
                        string commandText = reqParts[2];
                        string? param = reqParts.Length > 3 ? reqParts[3] : null;
                        if (!SerialCommandExtensions.TryParseWireString(commandText, out var command))
                        {
                            sp.WriteLine($"ACK,{requestId}");
                            sp.WriteLine("UNKNOWN_COMMAND");
                            return;
                        }
                        sp.WriteLine($"ACK,{requestId}");
                        switch (command)
                        {
                            case SerialCommand.GET_DISTANCE:
                                if (string.IsNullOrWhiteSpace(param))
                                {
                                    sp.WriteLine("MISSING_ID");
                                    return;
                                }
                                var anchor = _simulationService.helicopter.GetAnchorById(param);
                                response = anchor != null
                                   ? $"DISTANCE,{requestId},{time},{_simulationService.CalaculateDistance(anchor)},{param}"
                                   : $"DISTANCE,{requestId},{time},NA,{param}";
                                break;

                            case SerialCommand.GET_PILOT_POSITION:
                                _simulationService.SetNewPilotCordinate(
                                    _timer.GetTimePassFromLast(),
                                    CordinateType.X,
                                    DirectionType.BACKWARD);

                                var point = _simulationService.GetPilotPoint();
                                response = $"PILOT_POSITION,{requestId},{time},{point.X},{point.Y},{point.Z}";
                                break;

                            case SerialCommand.GET_ANCHOR_POSITION:
                                if (string.IsNullOrWhiteSpace(param))
                                {
                                    sp.WriteLine("MISSING_ID");
                                    return;
                                }
                                var anchorPosition = _simulationService.helicopter.GetAnchorById(param);
                                response = anchorPosition != null
                                    ? $"ANCHOR_POSITION,{requestId},{time},{anchorPosition.Id},{anchorPosition.point.X},{anchorPosition.point.Y},{anchorPosition.point.Z}"
                                    : $"ANCHOR_POSITION,{requestId},{time},{param},NA,NA,NA";
                                break;

                            default:
                                sp.WriteLine("UNKNOWN_COMMAND");
                                break;
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(ConfigManager.GetDouble("RESPONSE_DELAY")));
                        sp.WriteLine(response);
                        _timer.SetLastTimer();



                    }
                    catch (TimeoutException) { }
                };
                Console.WriteLine("Slave is listening...");
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }


    }
}
