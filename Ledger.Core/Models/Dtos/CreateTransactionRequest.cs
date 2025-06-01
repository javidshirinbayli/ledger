namespace Ledger.Core.Models.Dtos;

public record CreateTransactionRequest(
    TransactionType Type,
    decimal Amount,
    string? Description = null
);