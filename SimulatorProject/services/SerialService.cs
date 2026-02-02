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
                        string[] parts = request.Split(':', 2);
                        string commandText = parts[0];
                        string response = "";
                        string time = _timer.GetElapsedTime();
                        if (!SerialCommandExtensions.TryParseWireString(commandText, out var command))
                        {
                            sp.WriteLine("UNKNOWN_COMMAND");
                            return;
                        }
                        switch (command)
                        {
                            case SerialCommand.GET_DISTANCE:
                                if (parts.Length < 2)
                                {
                                    sp.WriteLine("MISSING_ID");
                                    return;
                                }
                                var anchor = _simulationService.helicopter.GetAnchorById(parts[1]);
                                response = anchor != null
                                   ? $"DISTANCE,{time},{_simulationService.CalaculateDistance(anchor)},{parts[1]}"
                                   : $"DISTANCE,{time},NA,{parts[1]}";
                                break;

                            case SerialCommand.GET_PILOT_POSITION:
                                _simulationService.SetNewPilotCordinate(
                                    _timer.GetTimePassFromLast(),
                                    CordinateType.X,
                                    DirectionType.BACKWARD);

                                var point = _simulationService.GetPilotPoint();
                                response = $"PILOT_POSITION,{time},{point.X},{point.Y},{point.Z}";
                                break;

                            case SerialCommand.GET_ANCHOR_POSITION:
                                if (parts.Length < 2)
                                {
                                    sp.WriteLine("MISSING_ID");
                                    return;
                                }
                                var anchorPosition = _simulationService.helicopter.GetAnchorById(parts[1]);
                                response = anchorPosition != null
                                    ? $"ANCHOR_POSITION,{time},{anchorPosition.Id},{anchorPosition.point.X},{anchorPosition.point.Y},{anchorPosition.point.Z}"
                                    : $"ANCHOR_POSITION,{time},{parts[1]},NA,NA,NA";
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
