using FluentValidation;
using Ledger.Core.Exceptions;
using Ledger.Core.Models;
using Ledger.Core.Models.Dtos;
using Ledger.Core.Services;

namespace Ledger.Api;

public static class Endpoints
{
    public static void AddLedgerEndpoints(this WebApplication app)
    {
        app.MapPost("/accounts", async (
                CreateAccountRequest request,
                ILedgerService ledgerService,
                IValidator<CreateAccountRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.BadRequest(validationResult.Errors);

                var account = await ledgerService.CreateAccountAsync(request);
                return Results.Created($"/accounts/{account.Id}", account);
            })
            .WithName("CreateAccount")
            .WithSummary("Create an account")
            .WithDescription("Creates a new account with an initial balance of 0.")
            .Produces<Account>(201)
            .Produces<IEnumerable<FluentValidation.Results.ValidationFailure>>(400);

        app.MapGet("/accounts/{accountId}", async (
                string accountId,
                ILedgerService ledgerService) =>
            {
                var account = await ledgerService.GetAccountAsync(accountId);
                return account is not null ? Results.Ok(account) : Results.NotFound();
            })
            .WithName("GetAccount")
            .WithSummary("Get account details")
            .WithDescription("Retrieves account information for the specified account ID.")
            .Produces<Account>(200)
            .Produces(404);

        app.MapGet("/accounts/{accountId}/balance", async (
                string accountId,
                ILedgerService ledgerService) =>
            {
                var balance = await ledgerService.GetAccountBalanceAsync(accountId);
                return balance is not null ? Results.Ok(balance) : Results.NotFound();
            })
            .WithName("GetAccountBalance")
            .WithSummary("Get account balance")
            .WithDescription("Retrieves the current balance for the specified account ID.")
            .Produces<AccountBalanceResponse>(200)
            .Produces(404);

        app.MapPost("/accounts/{accountId}/transactions", async (
                string accountId,
                CreateTransactionRequest request,
                ILedgerService ledgerService,
                IValidator<CreateTransactionRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.BadRequest(validationResult.Errors);

                try
                {
                    var transaction = await ledgerService.RecordTransactionAsync(accountId, request);
                    return Results.Created($"/accounts/{accountId}/transactions/{transaction.Id}", transaction);
                }
                catch (AccountNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (InsufficientFundsException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (LedgerException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("CreateTransaction")
            .WithSummary("Record a deposit or withdrawal")
            .WithDescription("Records a transaction (deposit or withdrawal) for the specified account.")
            .Produces<TransactionResponse>(201)
            .Produces(400)
            .Produces(404);

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
            .WithName("GetTransactionHistory")
            .WithSummary("Get transaction history")
            .WithDescription("Retrieves all transactions for the specified account, ordered by date.")
            .Produces<IEnumerable<TransactionResponse>>(200)
            .Produces(404);
        
        app.MapPost("/accounts/{fromAccountId}/transfer", async (
                TransferRequest request,
                ILedgerService ledgerService,
                IValidator<TransferRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.BadRequest(validationResult.Errors);

                try
                {
                    var transactions = await ledgerService.Transfer(request);
                    return Results.Created($"/accounts/{request.FromAccountId}/transfer", transactions);
                }
                catch (AccountNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (InsufficientFundsException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (LedgerException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("Transfer")
            .WithSummary("Transfer Money Between Accounts")
            .WithDescription("Records a transactions (deposit and withdrawal) for the specified accounts.")
            .Produces<TransactionResponse>(201)
            .Produces(400)
            .Produces(404);
    }
}
