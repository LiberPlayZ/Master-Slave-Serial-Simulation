using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimulationMaster.config
{
    public class ConfigData
    {


        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public string GetDistanceCommand { get; set; }
        public ConfigData(string pn, int baudRate, string getDistance)
        {
            this.PortName = pn;
            this.BaudRate = baudRate;
            this.GetDistanceCommand = getDistance;
        }
    }
}