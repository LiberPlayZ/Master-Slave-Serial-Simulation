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
                                string anchorId = parts[1];
                                string time = _timer.GetElapsedTime();

                                _simulationService.SetNewPilotCordinate(
                                    _timer.GetTimePassFromLast(),
                                    CordinateType.X,
                                    DirectionType.BACKWARD);

                                var anchor = _simulationService.helicopter.GetAnchorById(anchorId);
                                response = anchor != null
                                   ? $"Time: {time},distance: {_simulationService.CalaculateDistance(anchor)},Id: {anchorId}"
                                   : $"Time: {time},No anchor found";
                                break;

                            case SerialCommand.GET_PILOT_POSITION:
                                _simulationService.SetNewPilotCordinate(
                                    _timer.GetTimePassFromLast(),
                                    CordinateType.X,
                                    DirectionType.BACKWARD);

                                var point = _simulationService.GetPilotPoint();
                                response = $"PILOT_POSITION:{point.X},{point.Y},{point.Z}";
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