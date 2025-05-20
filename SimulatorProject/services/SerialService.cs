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

        public SerialService(TimerService timer, ConfigData config)
        {
            _timer = timer;
            _serialPort = new SerialPort(config.PortName.Trim(), config.BaudRate)
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
                Console.WriteLine("SerialTimerServer is running and waiting for requests...");

                while (true)
                {
                    try
                    {
                        string request = _serialPort.ReadLine().Trim();

                        if (request == "GET_TIME")
                        {
                            string time = _timer.GetElapsedTime();
                            _serialPort.WriteLine(time);
                            Console.WriteLine($"Sent: {time}");
                        }
                        else
                        {
                            _serialPort.WriteLine("UNKNOWN_COMMAND");
                        }
                    }
                    catch (TimeoutException)
                    {

                    }
                    Thread.Sleep(100);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }
    }
}