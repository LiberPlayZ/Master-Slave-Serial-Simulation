
using SimulatorProject.services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using SimulatorProject.Models;
using SimulatorProject.config;
class Program
{
    static void Main(string[] args)
    {
        var json = File.ReadAllText("config/SimulationConfig.json");
        var config = JsonSerializer.Deserialize<SimulationConfig>(json);
        if (config != null)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(config);

                    services.AddSingleton<TimerService>();
                    services.AddSingleton<SerialService>();
                    services.AddSingleton<SimulationService>();
                })
                .Build();


            var serialService = host.Services.GetRequiredService<SerialService>();
            serialService.Start();
        }
        else
        {
            System.Console.WriteLine("No json provide with deffualt values.");

        }







    }
}
