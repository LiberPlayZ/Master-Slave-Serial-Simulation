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

        public Anchor? GetAnchorById(string id)
        {
            foreach (var anchor in this.anchors)
            {
                if (anchor.Id == id)
                {
                    return anchor;
                }
            }
            return null;
        }

    }
}