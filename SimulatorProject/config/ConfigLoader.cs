using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotenv.net;
using SimulatorProject.utils.mappers;

namespace SimulatorProject.config
{
    public static class ConfigLoader
    {

        public static ConfigData? LoadConfig()
        {
            DotEnv.Load();
            // load env
            var max_range = ParserMapper.DoubleParse(Environment.GetEnvironmentVariable("MAX_RANGE"));
            var min_range = ParserMapper.DoubleParse(Environment.GetEnvironmentVariable("MIN_RANGE"));
            var port_name = Environment.GetEnvironmentVariable("PORT_NAME");
            var baud_rate = ParserMapper.DoubleParse(Environment.GetEnvironmentVariable("Baud_Rate"));
            if (!max_range.HasValue || !min_range.HasValue || !baud_rate.HasValue || port_name == null || port_name.Trim() == "")
            {
                Console.WriteLine(" error load env types ");
                return null;
            }

            Console.WriteLine($"Using max range: {max_range} , min range {min_range} , port name {port_name} , baud rate {baud_rate} ");

            return new ConfigData((double)max_range, (double)min_range, port_name, (int)baud_rate);

        }
    }
}