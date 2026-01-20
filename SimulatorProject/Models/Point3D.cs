using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.Models
{
    public class Point3D(double x = 0, double y = 0, double z = 0)
    {


        public double X { get; set; } = x;

        public double Y { get; set; } = y;

        public double Z { get; set; } = z;

        public override string ToString()
        {
            return $"({this.X},{this.Y},{this.Z})";
        }
    }
}