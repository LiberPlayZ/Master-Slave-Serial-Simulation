using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotenv.net;
using SharedConfig;
namespace SimulatorProject.config
{
    public static class ConfigLoader
    {

        public static ConfigData? LoadConfig()
        {
            SharedConfig.ConfigData.Load();
            // load env
            var max_range = SharedConfig.ConfigData.GetDouble(SharedConfig.ConfigData.Get("MAX_RANGE"));
            var min_range = SharedConfig.ConfigData.GetDouble(SharedConfig.ConfigData.Get("MIN_RANGE"));
            var port_name = SharedConfig.ConfigData.Get("PORT_NAME");
            var baud_rate = SharedConfig.ConfigData.GetInt(SharedConfig.ConfigData.Get("BAUD_RATE"));
            var get_distance_command = SharedConfig.ConfigData.Get("GET_DISTANCE_COMMAND");
            var resposnse_delay = SharedConfig.ConfigData.GetDouble(SharedConfig.ConfigData.Get("RESPONSE_DELAY"));


            if (!max_range.HasValue || !min_range.HasValue || !baud_rate.HasValue || port_name == null || port_name.Trim() == ""
             || get_distance_command == null || get_distance_command.Trim() == "" || !resposnse_delay.HasValue)
            {
                Console.WriteLine(" error load env types ");
                return null;
            }

            Console.WriteLine($"Using max range: {max_range} , min range {min_range} , port name {port_name} , baud rate {baud_rate} , delay {resposnse_delay} ");

            return new ConfigData((double)max_range, (double)min_range, port_name, (int)baud_rate, get_distance_command, (double)resposnse_delay);

        }
    }
}