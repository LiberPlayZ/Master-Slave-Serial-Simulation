using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.Models
{
    public class Pilot
    {
        public Point3D point { get; set; }
        public Pilot()
        {

        }
        public Pilot(double x, double y, double z)
        {
            this.point = new Point3D(x, y, z);
        }
        public Pilot(Pilot other)
        {
            this.point = new Point3D(other.point.X, other.point.Y, other.point.Z);
        }

    }
}