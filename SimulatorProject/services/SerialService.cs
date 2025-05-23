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

        private double GetDistance(string id)
        {
            var anchor = this._simulationService.helicopter.GetAnchorById(id);
            if (anchor != null)
            {
                return this._simulationService.CalaculateDistance(anchor);
            }
            return 0.0;
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
                            double distance = this.GetDistance(data[1]);



                            await Task.Delay(TimeSpan.FromMilliseconds(this._responseDelay));

                            sp.WriteLine(time);
                            Console.WriteLine($"Sent: {time}");


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