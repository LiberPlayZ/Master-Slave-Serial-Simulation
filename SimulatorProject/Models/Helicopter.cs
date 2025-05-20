using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.Models
{
    public class Helicopter
    {
        public Anchor[] anchors;
        public Helicopter()
        {
            this.anchors = new Anchor[3];
        }

    }
}