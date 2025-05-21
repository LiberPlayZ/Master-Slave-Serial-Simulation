using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.Models
{
    public class Pilot(double x = 1, double y = 1, double z = 0)
    {
        public Point3D point = new Point3D(x, y, z);
    }
}