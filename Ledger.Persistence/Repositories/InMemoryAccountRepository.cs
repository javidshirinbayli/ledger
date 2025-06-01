using System.Collections.Concurrent;
using Ledger.Core.Models;
using Ledger.Core.Repositories;

namespace Ledger.Persistence.Repositories;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly ConcurrentDictionary<string, Account> _accounts = new();

    public Task<Account> AddAsync(Account account)
    {
        _accounts[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task<Account?> GetByIdAsync(string accountId)
    {
        _accounts.TryGetValue(accountId, out var account);
        return Task.FromResult(account);
    }

    public Task<IEnumerable<Account>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Account>>(_accounts.Values.ToList());
    }

    public Task UpdateAsync(Account account)
    {
        _accounts[account.Id] = account;
        return Task.CompletedTask;
    }
}