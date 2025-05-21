using SimulationMaster.config;
using SimulationMaster.services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
class Program
{
    static void Main(string[] args)
    {
        var config = ConfigLoader.LoadConfig();
        if (config != null)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(config);
                    services.AddSingleton<SerialService>();

                })
                .Build();
            var serialService = host.Services.GetRequiredService<SerialService>();
            serialService.Start();


        }

        // string portName = "/tmp/ttyV1"; // this should be the master side of your virtual port
        // int baudRate = 9600;

        // using var serialPort = new SerialPort(portName, baudRate);
        // serialPort.Open();

        // Console.WriteLine("Sending time request to slave...");
        // serialPort.WriteLine("GET_TIME");

        // string response = serialPort.ReadLine();
        // Console.WriteLine($"Received time from slave: {response}");






    }
}

