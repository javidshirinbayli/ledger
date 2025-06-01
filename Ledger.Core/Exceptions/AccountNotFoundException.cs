namespace Ledger.Core.Exceptions;

public class AccountNotFoundException(string accountId)
    : LedgerException($"Account with ID '{accountId}' was not found.");