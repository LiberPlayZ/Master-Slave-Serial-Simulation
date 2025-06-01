using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimulatorProject.Models;

namespace SimulatorProject.services
{
    public class SimulationService
    {

        private Pilot pilot;

        public Helicopter helicopter;

        private double error;

        private double pilot_speed;

        public Random random = new Random();
        public SimulationService()
        {
            this.pilot = new Pilot();
            this.helicopter = new Helicopter();
            this.error = 0.0;
            this.pilot_speed = SharedConfig.ConfigManager.GetDouble("PILOT_SPEED");
          

        }

        public void GenerateError(double min, double max)
        {
            this.error = min + (random.NextDouble() * (max - min));
        }


        public double CalaculateDistance(Anchor anchor)
        {
            double sideA = Math.Max(Math.Abs(this.pilot.point.Y), Math.Abs(anchor.point.Y))
             - Math.Min(Math.Abs(this.pilot.point.Y), Math.Abs(anchor.point.Y));

            double sideB = Math.Max(Math.Abs(this.pilot.point.X), Math.Abs(anchor.point.X))
            - Math.Min(Math.Abs(this.pilot.point.X), Math.Abs(anchor.point.X));

             double sideC = Math.Max(Math.Abs(this.pilot.point.Z), Math.Abs(anchor.point.Z))
            - Math.Min(Math.Abs(this.pilot.point.Z), Math.Abs(anchor.point.Z));

            return Math.Sqrt(Math.Pow(sideA, 2) + Math.Pow(sideB, 2) + Math.Pow(sideC, 2));
        }
    }
}