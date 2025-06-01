namespace Ledger.Core.Models.Dtos;

public record TransactionResponse(
    string Id,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateTime Timestamp
);