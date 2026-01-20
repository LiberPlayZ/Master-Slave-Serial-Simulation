using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimulatorProject.Models;

namespace SimulatorProject.config
{
    public class SimulationConfig
    {
        public required Anchor[] Anchors { get; set; }
        public required Pilot Pilot { get; set; }
    }
}