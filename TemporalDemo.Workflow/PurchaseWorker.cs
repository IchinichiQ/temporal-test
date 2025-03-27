using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Worker;

namespace TemporalDemo.Workflow;
public sealed class PurchaseWorker : BackgroundService
{
    private readonly ILoggerFactory _loggerFactory;

    public PurchaseWorker(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var activities = new PurchaseActivities();
        
        using var worker = new TemporalWorker(
            await TemporalClient.ConnectAsync(new()
            {
                TargetHost = "localhost:7233",
                LoggerFactory = _loggerFactory,
            }),
            new TemporalWorkerOptions(taskQueue: TasksQueue.ProcessOrder)
                .AddActivity(activities.CreateOrder)
                .AddActivity(activities.CheckPayment)
                .AddActivity(activities.CheckInventory)
                .AddActivity(activities.FulfillOrder)
                .AddActivity(activities.ShipOrder)
                .AddWorkflow<ProcessOrderWorkflow>()
            );
        
        Console.WriteLine("Running worker");
        try
        {
            await worker.ExecuteAsync(stoppingToken);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine("Worker cancelled");
        }
    }
}