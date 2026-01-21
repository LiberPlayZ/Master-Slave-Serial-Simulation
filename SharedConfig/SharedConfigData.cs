using dotenv.net;
using System;
using System.Collections.Generic;

namespace SharedConfig
{
    public static class ConfigManager
    {
        private static readonly Dictionary<string, string> _config;

        static ConfigManager()
        {

            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            _config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["SLAVE_PORT_NAME"] = Environment.GetEnvironmentVariable("SLAVE_PORT_NAME") ?? " /tmp/ttyV0",
                ["MASTER_PORT_NAME"] = Environment.GetEnvironmentVariable("MASTER_PORT_NAME") ?? " /tmp/ttyV1",
                ["BAUD_RATE"] = Environment.GetEnvironmentVariable("BAUD_RATE") ?? "9600",
                ["RESPONSE_DELAY"] = Environment.GetEnvironmentVariable("RESPONSE_DELAY") ?? "5.5",
                ["MAX_RANGE"] = Environment.GetEnvironmentVariable("MAX_RANGE") ?? "0.5",
                ["MIN_RANGE"] = Environment.GetEnvironmentVariable("MIN_RANGE") ?? "-0.5",
                ["CSV_PATH"] = Environment.GetEnvironmentVariable("CSV_PATH") ?? "logs/output/log.csv",
                ["PILOT_SPEED"] = Environment.GetEnvironmentVariable("PILOT_SPEED") ?? "10",
                ["PILOT_SPEED_TYPE"] = Environment.GetEnvironmentVariable("PILOT_SPEED_TYPE") ?? "mps",



            };
        }

        public static string Get(string key) => _config[key];

        public static int GetInt(string key) => int.Parse(_config[key]);

        public static double GetDouble(string key) => double.Parse(_config[key]);
    }
}
