using System.Collections.Concurrent;
using Ledger.Core.Models;
using Ledger.Core.Repositories;

namespace Ledger.Persistence.Repositories;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly ConcurrentBag<Transaction> _transactions = new();

    public Task<Transaction> AddAsync(Transaction transaction)
    {
        _transactions.Add(transaction);
        return Task.FromResult(transaction);
    }

    public Task<IEnumerable<Transaction>> GetByAccountIdAsync(string accountId)
    {
        var filtered = _transactions
            .Where(t => t.AccountId == accountId)
            .ToList();
        
        return Task.FromResult<IEnumerable<Transaction>>(filtered);
    }

    public Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Transaction>>(_transactions.ToList());
    }
}