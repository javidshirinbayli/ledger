namespace Ledger.Core.Exceptions;

public class InsufficientFundsException(
    string accountId,
    decimal balance,
    decimal withdrawal
) : LedgerException(
    $"Insufficient funds in account '{accountId}'. Current balance: {balance}, withdrawal amount: {withdrawal}.");