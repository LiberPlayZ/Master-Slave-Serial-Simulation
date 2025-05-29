using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using SharedConfig;

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
                        if (request.StartsWith(SharedConfig.ConfigManager.Get("GET_DISTANCE_COMMAND").Trim() + ":"))
                        {
                            string[] data = request.Split(':');
                            this._simulationService.helicopter.ChangeRandomAnchorPos();
                            string time = _timer.GetElapsedTime();
                            string response = "";
                            var anchor = this._simulationService.helicopter.GetAnchorById(data[1]);
                            if (anchor != null)
                            {
                                response = $"Time: {time},distance: {this._simulationService.CalaculateDistance(anchor)},Id: {data[1]}";

                            }
                            else
                            {
                                response = $"Time: {time},No anchor found";

                            }



                            await Task.Delay(TimeSpan.FromMilliseconds(SharedConfig.ConfigManager.GetDouble("RESPONSE_DELAY")));

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