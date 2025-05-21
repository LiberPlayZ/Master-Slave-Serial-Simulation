using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimulatorProject.Models;

namespace SimulatorProject.services
{
    public static class DIstanceService
    {
        public static double CalaculateDistance(Pilot pilot, Anchor anchor)
        {
            double sideA = Math.Max(Math.Abs(pilot.point.Y), Math.Abs(anchor.point.Y))
             - Math.Min(Math.Abs(pilot.point.Y), Math.Abs(anchor.point.Y));

            double sideB = Math.Max(Math.Abs(pilot.point.X), Math.Abs(anchor.point.X))
            - Math.Min(Math.Abs(pilot.point.X), Math.Abs(anchor.point.X));

            return Math.Sqrt(Math.Pow(sideA, 2) + Math.Pow(sideB, 2));
        }
    }
}