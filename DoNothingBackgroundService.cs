using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace PizzaHotOnion
{
  public class DoNothingBackgroundService : BackgroundService
  {
    public DoNothingBackgroundService()
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
      }
    }
  }
}
