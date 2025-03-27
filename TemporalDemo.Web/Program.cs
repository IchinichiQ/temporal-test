using TemporalDemo.Workflow;
using Temporalio.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole().SetMinimumLevel(LogLevel.Information);

builder.Services.AddSingleton(ctx =>
    TemporalClient.ConnectAsync(new()
    {
        TargetHost = "localhost:7233",
        LoggerFactory = ctx.GetRequiredService<ILoggerFactory>(),
    }));

builder.Services.AddHostedService<PurchaseWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/start", async (Task<TemporalClient> clientTask, string itemId, string userId, string address, bool isItemExist) =>
{
    var client = await clientTask;
    
    await client.StartWorkflowAsync(
        (ProcessOrderWorkflow wf) => wf.RunAsync(new(itemId, userId), isItemExist, address),
        new(id: $"process-order-number-{Guid.NewGuid()}", taskQueue: TasksQueue.ProcessOrder));
});

app.MapGet("/history", () =>
{
    return PurchaseStatusHistoryService.GetPurchaseStatusList();
});

app.MapGet("/clear", () =>
{
    PurchaseStatusHistoryService.Clear();
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();