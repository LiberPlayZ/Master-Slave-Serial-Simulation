using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.Models
{
    public class Anchor
    {
        public required string Id { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public Anchor(string id, int x, int y, int z)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        

    }
}