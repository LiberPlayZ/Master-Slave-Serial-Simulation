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

        public Random random = new Random();
        public Helicopter()
        {
            this.anchors = new Anchor[3];
            int id = 1;
            for (int i = 0; i < 3; i++)
            {
                this.anchors[i] = new Anchor(id.ToString());
                id++;
            }
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

        public void ChangeRandomAnchorPos()
        {
            int pos = random.Next(this.anchors.Length);
            this.anchors[pos].GenerateNewPoint(random, 20);
        }

        public override string ToString()
        {
            string res = "";
            foreach (var anchor in this.anchors)
            {
                res += anchor.ToString() + "\n";

            }
            return res;
        }

    }
}