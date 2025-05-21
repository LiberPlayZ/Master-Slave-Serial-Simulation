using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotenv.net;
using SharedConfig;



namespace SimulationMaster.config
{
    public static class ConfigLoader
    {

        public static ConfigData? LoadConfig()
        {
            SharedConfig.ConfigData.Load();

            // load env

            var port_name = SharedConfig.ConfigData.Get("PORT_NAME");
            var baud_rate = SharedConfig.ConfigData.GetInt(SharedConfig.ConfigData.Get("BAUD_RATE"));
            var get_distance_command = SharedConfig.ConfigData.Get("GET_DISTANCE_COMMAND");
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