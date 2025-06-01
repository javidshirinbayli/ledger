namespace Ledger.Core.Models.Dtos;

public record AccountBalanceResponse(
    string AccountId,
    string Name,
    decimal Balance,
    DateTime LastUpdated
);