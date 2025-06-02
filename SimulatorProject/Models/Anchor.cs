using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.Models
{
    public class Anchor
    {
        public string Id { get; set; }

        public Point3D point { get; set; }

        public Anchor()
        {
            
        }

        public Anchor(string id, double x = 1, double y = 1, double z = 1)
        {
            this.Id = id;
            this.point = new Point3D(x, y, z);
        }

        public Anchor(Anchor other)
        {
            this.Id = other.Id;
            this.point = new Point3D(other.point.X, other.point.Y, other.point.Z);
        }

        public override string ToString()
        {
            return $"Id:{this.Id} , " + this.point.ToString();
        }
        public void SetPoint(Point3D point3D)
        {
            this.point.X = point3D.X;
            this.point.Y = point3D.Y;
            this.point.Z = point3D.Z;
        }

        public void GenerateNewPoint(Random random, int max)
        {
            this.point.X = (double)random.Next(max);
            this.point.Y = (double)random.Next(max);
            this.point.Z = (double)random.Next(max);
        }
    }
}