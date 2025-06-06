using System.Transactions;
using Ledger.Core.Exceptions;
using Ledger.Core.Models;
using Ledger.Core.Models.Dtos;
using Ledger.Core.Repositories;
using Transaction = Ledger.Core.Models.Transaction;

namespace Ledger.Core.Services;

public class LedgerService(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    TimeProvider timeProvider
)
    : ILedgerService
{
    public async Task<Account> CreateAccountAsync(CreateAccountRequest request)
    {
        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Balance = 0,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await accountRepository.AddAsync(account);
        return account;
    }

    public async Task<Account?> GetAccountAsync(string accountId)
    {
        return await accountRepository.GetByIdAsync(accountId);
    }

    public async Task<AccountBalanceResponse?> GetAccountBalanceAsync(string accountId)
    {
        var account = await accountRepository.GetByIdAsync(accountId);
        if (account == null)
            return null;

        return new AccountBalanceResponse(
            account.Id,
            account.Name,
            account.Balance,
            timeProvider.GetUtcNow().DateTime
        );
    }

    public async Task<Transaction> RecordTransactionAsync(string accountId, CreateTransactionRequest request)
    {
        var account = await accountRepository.GetByIdAsync(accountId);
        if (account == null)
            throw new AccountNotFoundException(accountId);

        if (request.Type == TransactionType.Withdrawal && account.Balance < request.Amount)
            throw new InsufficientFundsException(accountId, account.Balance, request.Amount);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = accountId,
            Type = request.Type,
            Amount = request.Amount,
            Description = request.Description ?? string.Empty,
            Timestamp = timeProvider.GetUtcNow().DateTime
        };

        if (request.Type == TransactionType.Deposit)
            account.Balance += request.Amount;
        else
            account.Balance -= request.Amount;

        await accountRepository.UpdateAsync(account);
        await transactionRepository.AddAsync(transaction);

        return transaction;
    }

    public async Task<IEnumerable<TransactionResponse>> GetTransactionHistoryAsync(string accountId)
    {
        var transactions = await transactionRepository.GetByAccountIdAsync(accountId);
        return transactions
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new TransactionResponse(
                t.Id,
                t.Type,
                t.Amount,
                t.Description,
                t.Timestamp
            ));
    }

    public async Task<Transaction[]> Transfer(TransferRequest request)
    {
        var transactionFromTask = RecordTransactionAsync(request.FromAccountId, new(
            TransactionType.Withdrawal,
            request.Amount,
            request.Description
        ));
        
        var transactionToTask = RecordTransactionAsync(request.ToAccountId, new(
            TransactionType.Deposit,
            request.Amount,
            request.Description
        ));

        return await Task.WhenAll(transactionFromTask, transactionToTask);
    }
}