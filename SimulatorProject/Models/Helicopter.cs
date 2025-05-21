using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.Models
{
    public class Helicopter
    {
        public Anchor[] anchors;

        public Point3D center;
        public Helicopter()
        {
            this.anchors = new Anchor[3];
            this.center = new Point3D();
        }

    }
}