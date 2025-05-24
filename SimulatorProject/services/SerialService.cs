using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using SimulatorProject.config;

namespace SimulatorProject.services
{
    public class SerialService
    {
        private readonly SerialPort _serialPort;
        private readonly TimerService _timer;

        private readonly SimulationService _simulationService;

        private readonly string _getDistance;

        private readonly double _responseDelay;

        public SerialService(TimerService timer, SimulationService simulationService, ConfigData config)
        {
            _timer = timer;
            this._simulationService = simulationService;
            _serialPort = new SerialPort(config.PortName.Trim(), config.BaudRate)
            {
                NewLine = "\n",
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };
            this._getDistance = config.GetDistanceCommand.Trim();
            this._responseDelay = config.ResponseDelay;
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
                        if (request.StartsWith(this._getDistance + ":"))
                        {
                            string[] data = request.Split(':');

                            string time = _timer.GetElapsedTime();
                            string response = "";
                            var anchor = this._simulationService.helicopter.GetAnchorById(data[1]);
                            System.Console.WriteLine(anchor);
                            if (anchor != null)
                            {
                                response = $"Time: {time} | " + anchor.ToString() + "|"
                               + $"distance: {this._simulationService.CalaculateDistance(anchor)}";

                            }
                            else
                            {
                                response = $"Time: {time} | No anchor found";

                            }



                            await Task.Delay(TimeSpan.FromMilliseconds(this._responseDelay));

                            sp.WriteLine(response);
                          

                        }
                        else
                        {
                            sp.WriteLine("UNKNOWN_COMMAND");
                        }

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