# Tiny Ledger API

A simple web API for a digital ledger. Implements deposit/withdrawal transactions, account balance, and transaction history. Built in C# with .NET 9.

---

## Features

* **Record money movements:** Deposit and withdraw money into accounts.
* **View current balance:** See live account balances.
* **Transaction history:** List all movements, sorted by time.
* **In-memory data store:** No external dependencies or setup required.

---

## How to Run

**Requirements:**

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

**Start the API:**

```bash
cd Ledger.Api
dotnet run
```

* By default, the API listens on `http://localhost:5076` (or as configured in `launchSettings.json`).

**Run tests:**

```bash
dotnet test
```

* Runs all business logic and validation unit tests.

---

## Example Usage

### **Create an account**

```http
POST /accounts
Content-Type: application/json

{
  "name": "Alice"
}
```

### **Deposit money**

```http
POST /accounts/{accountId}/transactions
Content-Type: application/json

{
  "amount": 100,
  "type": "deposit",
  "description": "A blue"
}
```

### **Withdraw money**

```http
POST /accounts/{accountId}/transactions
Content-Type: application/json

{
  "amount": 50,
  "type": "withdrawal",
  "description": "half"
}
```

### **Get current balance**

```http
GET /accounts/{accountId}/balance
```

### **Get transaction history**

```http
GET /accounts/{accountId}/transactions
```

---

## Configuration

* Transaction limits (e.g., max allowed amount) are set in `appsettings.json`:

  ```json
  "TransactionLimits": {
    "MaxAmount": 1000000
  }
  ```

---

## Testing

* **Unit tests** cover all core business logic and validation.

    * Run with `dotnet test`.
* **Why no endpoint or repository tests?**

    * **Endpoints**: For a project of this size and based on assignment scope, endpoints are thin wrappers over business logic. Their behavior is indirectly covered via service and validation tests.
    * **Repositories**: In-memory repositories have no logic. They’re trivial data stores, so unit testing them would add no real value. In a real app with persistent storage, repository testing would be important.

---

## Assumptions & Design Decisions

* All data is in-memory; **no persistence** beyond process lifetime.
* **No authentication/authorization**.
* **No logging or monitoring**.
* Uses .NET 9's built-in `TimeProvider` for testable and deterministic time handling.
* Validation leverages FluentValidation with config-driven limits.
* **Enum values** (like `type`) are exposed as strings (e.g., `"deposit"`, `"withdrawal"`) for easy API usage.

### **Why I Explicitly Check Account Existence in Get Transactions**

In this endpoint:

```csharp
app.MapGet("/accounts/{accountId}/transactions", async (
    string accountId,
    ILedgerService ledgerService) =>
{
    var account = await ledgerService.GetAccountAsync(accountId);
    if (account is null)
        return Results.NotFound(new { error = "Account not found" });

    var transactions = await ledgerService.GetTransactionHistoryAsync(accountId);
    return Results.Ok(transactions);
})
```

I explicitly check if the account exists **before** returning transactions to:

* **Fail fast and clearly** if the account does not exist, with a clear error message and a 404 response.
* **Avoid leaking implementation details** (such as returning an empty list for a non-existent account, which could confuse API consumers and suggest the account exists but has no transactions).
* **Provide predictable, explicit error handling** for better API usability and contract clarity.

### **How the Logic is Handled**

* **Endpoints** are thin wrappers; all business logic (validation, error handling) is in the service layer, ensuring clear separation of concerns.
* **Validation** is enforced using FluentValidation at the endpoint level, so invalid data never reaches business logic.
* **Business exceptions** (e.g., account not found, insufficient funds) are handled explicitly, producing appropriate error responses.
* **Time handling** is via injected `TimeProvider` for testability and determinism.

### **Repository Pattern: Why Interfaces and Implementations?**

* **IAccountRepository** and **ITransactionRepository** abstract away the storage details. This allows:

    * Clean architecture: The service layer depends on abstractions, not concrete classes.
    * Easy swapping of storage implementation (e.g., moving to a real DB in the future with no changes to core logic).
    * Better testability (you can mock repositories in unit tests).
* **Concrete implementations** (in-memory in this case) live in the persistence layer. They are not referenced by the core/business logic, maintaining strict separation.

### **Project Reference Structure**

* **Core does not reference Persistence or API:**

    * All core logic is in `Ledger.Core`, which has **no dependencies** on infrastructure or presentation layers.
    * This ensures clean separation, maintainability, and extensibility.

---

## Exception Types

* `AccountNotFoundException`: Thrown when an operation references a non-existent account.
* `InsufficientFundsException`: Thrown when a withdrawal would overdraw the account.
* `LedgerException`: Base exception for domain/business errors.

> **Note:**
> I personally like using the result pattern (returning error/success types) instead of throwing exceptions for business rule violations, as it makes error handling more explicit and composable. However, I kept this assessment as simple as possible and used exceptions for clarity and brevity, in line with typical patterns for projects of this size.

---

## Project Structure

```
Ledger.Api/          # Web API (entry point)
Ledger.Core/         # Business/domain logic, models, validation
Ledger.Persistence/  # In-memory repository implementations
Ledger.Tests/        # xUnit unit tests for core logic and validators
```

---

## Development Notes

* Modern, clean architecture. Services depend on abstractions, not implementations.
* Deterministic time with `TimeProvider` and `FakeTimeProvider` (for tests).
* Easily extensible. Add transaction types or persistence with minimal change.

---

## Assignment Summary

This project was built as a take-home assignment to demonstrate:

* Clean API design
* Testable, modular architecture
* Real-world validation, error handling, and documentation

---