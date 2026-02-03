
using System;
using System.Collections.Generic;
namespace SharedConfig
{
    public enum SerialCommand
    {
        GET_DISTANCE,
        GET_PILOT_POSITION,
        GET_ANCHOR_POSITION,
        GET_STATUS,
        SET_NOISE,
        SET_JITTER
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
                SerialCommand.GET_STATUS => "GET_STATUS",
                SerialCommand.SET_NOISE => "SET_NOISE",
                SerialCommand.SET_JITTER => "SET_JITTER",
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
                case "GET_STATUS":
                    command = SerialCommand.GET_STATUS;
                    return true;
                case "SET_NOISE":
                    command = SerialCommand.SET_NOISE;
                    return true;
                case "SET_JITTER":
                    command = SerialCommand.SET_JITTER;
                    return true;
                default:
                    command = default;
                    return false;
            }
        }

    }


}
