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
            this._serialPort = new SerialPort(config.PortName.Trim(), config.BaudRate);
            this._getDistanceCommand = config.GetDistanceCommand.Trim();

        }

        public void Start()
        {
            try
            {
                this._serialPort.Open();
                Console.WriteLine("Sending time request to slave...");
                this._serialPort.WriteLine(this._getDistanceCommand);

                string response = this._serialPort.ReadLine();
                Console.WriteLine($"Received time from slave: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }
    }
}