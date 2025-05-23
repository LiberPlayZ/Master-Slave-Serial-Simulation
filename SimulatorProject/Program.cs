using SimulatorProject.config;
using SimulatorProject.services;
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
                    services.AddSingleton<TimerService>();
                    services.AddSingleton<SerialService>();
                    services.AddSingleton<SimulationService>();
                })
                .Build();

            var serialService = host.Services.GetRequiredService<SerialService>();
            serialService.Start();
        }






    }
}
