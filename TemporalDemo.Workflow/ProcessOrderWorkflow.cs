using Temporalio.Workflows;

namespace TemporalDemo.Workflow;

[Workflow]
public class ProcessOrderWorkflow
{
    [WorkflowRun]
    public async Task<PurchaseStatus> RunAsync(
        Purchase purchase,
        bool isItemExist,
        string address)
    {
        var orderId = await CreateOrder(purchase);

        var isExists = await CheckInventory(isItemExist);
        if (!isExists)
        {
            PurchaseStatusHistoryService.SetPurchaseStatus(PurchaseStatus.Cancelled);
            return PurchaseStatus.Cancelled;
        }

        await CheckPayment(orderId);

        await FulfillOrder(orderId);
        
        await ShipOrder(orderId, address);
        
        PurchaseStatusHistoryService.SetPurchaseStatus(PurchaseStatus.Completed);
        return PurchaseStatus.Completed;
    }

    private async Task<int> CreateOrder(Purchase purchase)
    {
        var orderId = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.CreateOrder(purchase),
            new()
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(90),
                RetryPolicy = new()
                {
                    InitialInterval = TimeSpan.FromSeconds(15),
                    BackoffCoefficient = 2,
                    MaximumInterval = TimeSpan.FromMinutes(1),
                    MaximumAttempts = 10
                }
            });

        return orderId;
    }
    
    private async Task<bool> CheckInventory(bool isItemExist)
    {
        var isExists = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.CheckInventory(isItemExist),
            new()
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(90),
            });

        return isExists;
    }

    private async Task CheckPayment(int orderId)
    {
        await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.CheckPayment(orderId),
            new()
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(3),
            });
    }

    private async Task FulfillOrder(int orderId)
    {
        await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.FulfillOrder(orderId),
            new()
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(90),
            });
    }

    private async Task ShipOrder(int orderId, string address)
    {
        await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
            (PurchaseActivities act) => act.ShipOrder(orderId, address),
            new()
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(90),
            });
    }
}