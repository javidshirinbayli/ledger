namespace Ledger.Core.Models.Dtos;

public record TransferRequest(string FromAccountId, string ToAccountId, decimal Amount, string? Description = null);