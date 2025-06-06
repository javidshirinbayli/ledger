using Ledger.Core.Models;
using Ledger.Core.Models.Dtos;

namespace Ledger.Core.Services;

public interface ILedgerService
{
    Task<Account> CreateAccountAsync(CreateAccountRequest request);
    Task<Account?> GetAccountAsync(string accountId);
    Task<AccountBalanceResponse?> GetAccountBalanceAsync(string accountId);
    Task<Transaction> RecordTransactionAsync(string accountId, CreateTransactionRequest request);
    Task<IEnumerable<TransactionResponse>> GetTransactionHistoryAsync(string accountId);
    Task<Transaction[]> Transfer(TransferRequest request);
}