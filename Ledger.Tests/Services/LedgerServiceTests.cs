using Ledger.Core.Exceptions;
using Ledger.Core.Models;
using Ledger.Core.Models.Dtos;
using Ledger.Core.Repositories;
using Ledger.Core.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Time.Testing;

namespace Ledger.Tests.Services;

public class LedgerServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly Mock<ITransactionRepository> _transactionRepoMock = new();
    private readonly FakeTimeProvider _fakeTimeProvider = new();
    private LedgerService CreateService() => new(_accountRepoMock.Object, _transactionRepoMock.Object, _fakeTimeProvider);

    [Fact]
    public async Task CreateAccountAsync_Creates_Account_With_Zero_Balance()
    {
        var testNow = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);
        _fakeTimeProvider.SetUtcNow(testNow);
        
        var service = CreateService();
        var request = new CreateAccountRequest(Name: "User");

        Account? addedAccount = null;
        _accountRepoMock.Setup(x => x.AddAsync(It.IsAny<Account>()))
            .Returns<Account>(acct =>
            {
                addedAccount = acct;
                return Task.FromResult(acct);
            });

        var account = await service.CreateAccountAsync(request);

        Assert.NotNull(account);
        Assert.Equal("User", account.Name);
        Assert.Equal(0, account.Balance);
        Assert.NotNull(account.Id);
        Assert.True(account.CreatedAt <= testNow);

        Assert.Equal(account, addedAccount);
    }

    [Fact]
    public async Task GetAccountAsync_Returns_Account_If_Exists()
    {
        var testNow = new DateTime(2025, 1, 2, 3, 4, 5);
        _fakeTimeProvider.SetUtcNow(testNow);
        
        var account = new Account { Id = "acc", Name = "User", Balance = 100, CreatedAt = testNow };
        _accountRepoMock.Setup(x => x.GetByIdAsync("acc")).ReturnsAsync(account);

        var service = CreateService();
        var result = await service.GetAccountAsync("acc");

        Assert.Equal(account, result);
    }

    [Fact]
    public async Task GetAccountAsync_Returns_Null_If_Not_Found()
    {
        _accountRepoMock.Setup(x => x.GetByIdAsync("none")).ReturnsAsync((Account?)null);

        var service = CreateService();
        var result = await service.GetAccountAsync("none");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccountBalanceAsync_Returns_Null_If_Not_Found()
    {
        _accountRepoMock.Setup(x => x.GetByIdAsync("none")).ReturnsAsync((Account?)null);
        var service = CreateService();

        var balance = await service.GetAccountBalanceAsync("none");
        Assert.Null(balance);
    }

    [Fact]
    public async Task GetAccountBalanceAsync_Returns_Balance()
    {
        var testNow = new DateTime(2025, 1, 2, 3, 4, 5);
        _fakeTimeProvider.SetUtcNow(testNow);
        
        var account = new Account { Id = "id", Name = "User", Balance = 999, CreatedAt = testNow };
        _accountRepoMock.Setup(x => x.GetByIdAsync("id")).ReturnsAsync(account);
        var service = CreateService();

        var balance = await service.GetAccountBalanceAsync("id");

        Assert.NotNull(balance);
        Assert.Equal(account.Balance, balance.Balance);
    }

    [Fact]
    public async Task RecordTransactionAsync_Throws_If_Account_Not_Found()
    {
        _accountRepoMock.Setup(x => x.GetByIdAsync("none")).ReturnsAsync((Account?)null);
        var service = CreateService();
        var request = new CreateTransactionRequest(Amount: 100, Type: TransactionType.Deposit);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            service.RecordTransactionAsync("none", request));
    }

    [Fact]
    public async Task RecordTransactionAsync_Throws_If_Insufficient_Funds()
    {
        var testNow = new DateTime(2025, 1, 2, 3, 4, 5);
        _fakeTimeProvider.SetUtcNow(testNow);
        
        var account = new Account { Id = "acc", Name = "User", Balance = 10, CreatedAt = testNow };
        _accountRepoMock.Setup(x => x.GetByIdAsync("acc")).ReturnsAsync(account);
        var service = CreateService();

        var request = new CreateTransactionRequest(Amount: 100, Type: TransactionType.Withdrawal);

        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            service.RecordTransactionAsync("acc", request));
    }

    [Fact]
    public async Task RecordTransactionAsync_Deposit_Increases_Balance()
    {
        var testNow = new DateTime(2025, 1, 2, 3, 4, 5);
        _fakeTimeProvider.SetUtcNow(testNow);
        
        var account = new Account { Id = "acc", Name = "User", Balance = 100, CreatedAt = testNow };
        _accountRepoMock.Setup(x => x.GetByIdAsync("acc")).ReturnsAsync(account);

        _transactionRepoMock.Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Returns<Transaction>(Task.FromResult);
        _accountRepoMock.Setup(x => x.UpdateAsync(account)).Returns(Task.CompletedTask);

        var service = CreateService();
        var request = new CreateTransactionRequest(Amount: 50, Type: TransactionType.Deposit);

        var transaction = await service.RecordTransactionAsync("acc", request);

        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(150, account.Balance);
    }

    [Fact]
    public async Task RecordTransactionAsync_Withdrawal_Decreases_Balance()
    {
        var testNow = new DateTime(2025, 1, 2, 3, 4, 5);
        _fakeTimeProvider.SetUtcNow(testNow);
        
        var account = new Account { Id = "acc", Name = "User", Balance = 100, CreatedAt = testNow };
        _accountRepoMock.Setup(x => x.GetByIdAsync("acc")).ReturnsAsync(account);

        _transactionRepoMock.Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Returns<Transaction>(Task.FromResult);
        _accountRepoMock.Setup(x => x.UpdateAsync(account)).Returns(Task.CompletedTask);

        var service = CreateService();
        var request = new CreateTransactionRequest(Amount: 80, Type: TransactionType.Withdrawal);

        var transaction = await service.RecordTransactionAsync("acc", request);

        Assert.Equal(TransactionType.Withdrawal, transaction.Type);
        Assert.Equal(20, account.Balance);
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_Returns_Transactions_For_Account()
    {
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = "t1", AccountId = "acc", Type = TransactionType.Deposit, Amount = 50,
                Description = "desc", Timestamp = DateTime.UtcNow.AddMinutes(-10)
            },
            new Transaction
            {
                Id = "t2", AccountId = "acc", Type = TransactionType.Withdrawal, Amount = 30,
                Description = "desc2", Timestamp = DateTime.UtcNow
            }
        };

        _transactionRepoMock.Setup(x => x.GetByAccountIdAsync("acc")).ReturnsAsync(transactions);

        var service = CreateService();
        var result = (await service.GetTransactionHistoryAsync("acc")).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("t2", result[0].Id);
        Assert.Equal("t1", result[1].Id);
    }
}