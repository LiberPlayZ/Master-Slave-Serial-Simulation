
using SimulationMaster.services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
class Program
{
    static async Task Main(string[] args)
    {


        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {

                services.AddSingleton<SerialService>();
                services.AddSingleton<CsvService>();
            })
            .Build();
        var serialService = host.Services.GetRequiredService<SerialService>();
        await serialService.Start();



    }
}

