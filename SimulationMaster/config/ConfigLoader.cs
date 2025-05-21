using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotenv.net;

using SimulationMaster.utild.mappers;

namespace SimulationMaster.config
{
    public static class ConfigLoader
    {

        public static ConfigData? LoadConfig()
        {
            DotEnv.Load();
            // load env

            var port_name = Environment.GetEnvironmentVariable("PORT_NAME");
            var baud_rate = ParserMapper.IntParse(Environment.GetEnvironmentVariable("BAUD_RATE"));
            var get_distance_command = Environment.GetEnvironmentVariable("GET_DISTANCE");
            if (!baud_rate.HasValue || port_name == null || port_name.Trim() == ""
             || get_distance_command == null || get_distance_command.Trim() == "")
            {
                Console.WriteLine(" error load env types ");
                return null;
            }

            Console.WriteLine($" port name {port_name} , baud rate {baud_rate} ");

            return new ConfigData(port_name, (int)baud_rate, get_distance_command);

        }
    }
}