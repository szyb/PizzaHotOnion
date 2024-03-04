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
using Serilog;
using Serilog.Events;

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

      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File(
          "Logs/HotOnion-.log",
          outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fffff} {Level:u3}]{ThreadId}|{SourceContext}| {Message:l}{NewLine}{Exception}",
          rollingInterval: RollingInterval.Day)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
        .CreateLogger();
      Log.Logger.Information("Hi");

      Configuration = builder.Build();

      BuildWebHost(args).Run();
    }

    public static IHost BuildWebHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, logging) =>
            {
              logging.ClearProviders();
              logging.AddSerilog(Log.Logger);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            })
            .UseWindowsService()
            .Build();
  }
}
