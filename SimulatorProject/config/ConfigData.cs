using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.config
{
    public class ConfigData
    {
        public double MaxRange { get; set; }
        public double MinRange { get; set; }

        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public ConfigData(double max, double min, string pn, int baudRate)
        {
            this.MaxRange = max;
            this.MaxRange = min;
            this.PortName = pn;
            this.BaudRate = baudRate;
        }

    }
}