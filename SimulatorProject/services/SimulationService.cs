using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimulatorProject.config;
using SimulatorProject.enums;
using SimulatorProject.Models;

namespace SimulatorProject.services
{
    public class SimulationService
    {

        private Pilot pilot;

        public Helicopter helicopter;

        private double error;

        private double pilot_speed;

        private readonly string pilot_speed_type;

        private readonly double scaleToUnits = 0.5; // variable to scale distance from real world to units . 

        public Random random = new Random();
        public SimulationService(SimulationConfig simulationConfig)
        {
            this.pilot = new Pilot(simulationConfig.Pilot);
            this.helicopter = new Helicopter(simulationConfig.Anchors);
            this.error = 0.0;
            this.pilot_speed = SharedConfig.ConfigManager.GetDouble("PILOT_SPEED");
            this.pilot_speed_type = SetPilotSpeedType(SharedConfig.ConfigManager.Get("PILOT_SPEED_TYPE").ToLower());


        }

        public void GenerateError(double min, double max)
        {
            this.error = min + (random.NextDouble() * (max - min));
        }


        public Point3D GetPilotPoint()
        {
            return this.pilot.GetPilotPointCopy();
        }

        private int GetDirectionMultiplyer(DirectionType direction)
        {
            return direction switch
            {
                DirectionType.LEFT or DirectionType.DOWN or DirectionType.BACKWARD => -1,
                DirectionType.RIGHT or DirectionType.UP or DirectionType.FORWARD => 1,
                _ => throw new ArgumentException("Invalid direction")
            };

        }

        private static string SetPilotSpeedType(string type)
        {

            if (type == PilotSpeedType.KILOMETERS_PER_HOUR || type == PilotSpeedType.METERS_PER_SECOND ||
             type == PilotSpeedType.MILES_PER_HOUR)
            {
                return type;
            }
            System.Console.WriteLine("pilot speed type is incorrectly and set to mps by deffualt .");
            return PilotSpeedType.METERS_PER_SECOND;


        }

        public void SetNewPilotCordinate(TimeSpan timePassed, CordinateType cordinate, DirectionType direction)
        {
            double distancePass = this.CalaculateDistancePass(timePassed);

            double movement = distancePass * this.GetDirectionMultiplyer(direction);


            switch (cordinate)
            {
                case CordinateType.X:
                    this.pilot.point.X += movement;
                    break;
                case CordinateType.Y:
                    this.pilot.point.Y += movement;
                    break;
                case CordinateType.Z:
                    this.pilot.point.Z += movement;
                    break;
            }
            System.Console.WriteLine($"time pass: {timePassed} \n distancePass: {distancePass} \n pilot point: {this.pilot.point.ToString()}");
        }

        public double CalaculateDistancePass(TimeSpan timePassed)
        {

            if (this.pilot_speed_type == PilotSpeedType.METERS_PER_SECOND)
            {
                return this.pilot_speed * timePassed.TotalSeconds * this.scaleToUnits;
            }
            else
            {
                return this.pilot_speed * timePassed.TotalHours * this.scaleToUnits;
            }


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