using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.Models
{
    public class Anchor(string id , double x = 1, double y = 1, double z = 1)
    {
        public  string Id { get; set; } = id;

        public Point3D point = new Point3D(x, y, z);

        public override string ToString()
        {
            return $"Id:{this.Id} , " + this.point.ToString();
        }
    }
}