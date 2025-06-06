namespace Ledger.Core.Models;

public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); //TODO: Code review
    public string AccountId { get; set; } = string.Empty; //TODO: Code review
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}