
using System;
using System.Collections.Generic;
namespace SharedConfig
{
    public enum SerialCommand
    {
        GET_DISTANCE,
        GET_PILOT_POSITION,
        GET_ANCHOR_POSITION
    }

    public static class SerialCommandExtensions
    {
        public static string ToWireString(this SerialCommand command)
        {
            return command switch
            {
                SerialCommand.GET_DISTANCE => "GET_DISTANCE",
                SerialCommand.GET_PILOT_POSITION => "GET_PILOT_POSITION",
                SerialCommand.GET_ANCHOR_POSITION => "GET_ANCHOR_POSITION",
                _ => "UNKNOWN"
            };
        }

        public static bool TryParseWireString(string value, out SerialCommand command)
        {
            switch (value)
            {
                case "GET_DISTANCE":
                    command = SerialCommand.GET_DISTANCE;
                    return true;
                case "GET_PILOT_POSITION":
                    command = SerialCommand.GET_PILOT_POSITION;
                    return true;
                case "GET_ANCHOR_POSITION":
                    command = SerialCommand.GET_ANCHOR_POSITION;
                    return true;
                default:
                    command = default;
                    return false;
            }
        }

    }


}