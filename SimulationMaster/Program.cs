
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

            })
            .Build();
        var serialService = host.Services.GetRequiredService<SerialService>();
        await serialService.Start();



    }
}

