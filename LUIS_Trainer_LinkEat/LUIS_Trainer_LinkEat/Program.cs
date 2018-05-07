using System;
using System.IO;
using LUIS_Trainer_LinkEat.Models;
using LUIS_Trainer_LinkEat.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LUIS_Trainer_LinkEat
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
 
            IConfigurationRoot configuration = builder.Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<MealRepository>()
                .AddSingleton<SauceRepository>()
                .AddSingleton<VegetableRepository>()
                .AddSingleton<PlaceRepository>()
                .AddSingleton<LuisManager>()
                .AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")))
                .BuildServiceProvider();

            var manager = serviceProvider.GetService<LuisManager>();

            Console.WriteLine("Bot trainer v0.1 is ready!");
            Console.ReadLine();

            manager.AddUtterances().Wait();
        }
    }
}
