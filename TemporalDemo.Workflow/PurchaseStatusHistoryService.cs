namespace TemporalDemo.Workflow;

public record PurchaseStatusHistoryItem(string Status, DateTimeOffset Timestamp);

public static class PurchaseStatusHistoryService
{
    private static readonly List<PurchaseStatusHistoryItem> _history = new();

    public static void SetPurchaseStatus(PurchaseStatus status)
    {
        _history.Add(new PurchaseStatusHistoryItem(status.ToString(), DateTimeOffset.UtcNow));
    }

    public static List<PurchaseStatusHistoryItem> GetPurchaseStatusList()
    {
        return _history;
    }

    public static void Clear()
    {
        _history.Clear();
    }
}