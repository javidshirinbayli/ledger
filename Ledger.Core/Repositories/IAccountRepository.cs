using Ledger.Core.Models;

namespace Ledger.Core.Repositories;

public interface IAccountRepository
{
    Task<Account> AddAsync(Account account);
    Task<Account?> GetByIdAsync(string accountId);
    Task<IEnumerable<Account>> GetAllAsync();
    Task UpdateAsync(Account account);
}