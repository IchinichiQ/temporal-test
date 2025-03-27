using Temporalio.Activities;

namespace TemporalDemo.Workflow;

public class PurchaseActivities
{
    private int PaymentAttempts = 0;

    [Activity]
    public Task<int> CreateOrder(Purchase purchase)
    {
        var orderId = Random.Shared.Next();
        
        Console.WriteLine($"Created order with id {orderId}");

        PurchaseStatusHistoryService.SetPurchaseStatus(PurchaseStatus.Initiated);
        
        return Task.FromResult(orderId);
    }

    [Activity]
    public async Task CheckPayment(int orderId)
    {
        if (PaymentAttempts >= 3)
        {
            PurchaseStatusHistoryService.SetPurchaseStatus(PurchaseStatus.PaymentAccepted);

            Console.WriteLine("Payment successful");
        }
        else
        {
            PaymentAttempts += 1;

            throw new Exception("Payment failed");
        }
    }

    [Activity]
    public async Task<bool> CheckInventory(bool isItemExist)
    {
        if (!isItemExist)
        {
            Console.WriteLine("Item not exist");
            
            PurchaseStatusHistoryService.SetPurchaseStatus(PurchaseStatus.NotAvailiableInventory);
            
            return false;
        }
        
        Console.WriteLine("Item exists");
        
        PurchaseStatusHistoryService.SetPurchaseStatus(PurchaseStatus.AvailiableInventory);
            
        return true;
    }

    [Activity]
    public async Task FulfillOrder(int orderId)
    {
        // success the request
        PurchaseStatusHistoryService.SetPurchaseStatus(PurchaseStatus.Fulfilled);
    }

    [Activity]
    public async Task ShipOrder(int orderId, string address)
    {
        // success the request
        PurchaseStatusHistoryService.SetPurchaseStatus(PurchaseStatus.Shipped);
        
        Console.WriteLine($"Order shipped to address {address}");
    }
}