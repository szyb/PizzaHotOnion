using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Hosting;

namespace PizzaHotOnion
{
  public class Program
  {
    public static IConfigurationRoot Configuration { get; set; }

    public static void Main(string[] args)
    {
      Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

      var builder = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json");

      Configuration = builder.Build();

      BuildWebHost(args).Run();
    }

    public static IHost BuildWebHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, logging) =>
            {
              logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
              logging.AddConsole();
              logging.AddDebug();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            })
            .UseWindowsService()
            .Build();
  }
}
