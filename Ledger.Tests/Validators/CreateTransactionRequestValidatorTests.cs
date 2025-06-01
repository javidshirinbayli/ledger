using FluentValidation.TestHelper;
using Ledger.Core;
using Ledger.Core.Models.Dtos;
using Ledger.Core.Models;
using Ledger.Core.Validators;
using Microsoft.Extensions.Options;
using Xunit;

namespace Ledger.Tests.Validators;

public class CreateTransactionRequestValidatorTests
{
    private readonly decimal _maxAmount = 1000000;

    private CreateTransactionRequestValidator CreateValidator() =>
        new(Options.Create(new TransactionLimitsOptions(MaxAmount: _maxAmount)));

    [Fact]
    public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative()
    {
        var validator = CreateValidator();
        var model = new CreateTransactionRequest(Amount: 0, Type: TransactionType.Deposit);
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);

        model = model with { Amount = -100 };
        result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Have_Error_When_Amount_Exceeds_Max()
    {
        var validator = CreateValidator();
        var model = new CreateTransactionRequest(Amount: _maxAmount + 1, Type: TransactionType.Deposit);
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Too_Long()
    {
        var validator = CreateValidator();
        var model = new CreateTransactionRequest
        (
            Amount: 100,
            Type: TransactionType.Deposit,
            Description: new string('d', 501)
        );
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Empty()
    {
        var validator = CreateValidator();
        var model = new CreateTransactionRequest(Amount: 100, Type: TransactionType.Deposit, Description: "");
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Type_Is_Invalid()
    {
        var validator = CreateValidator();
        var model = new CreateTransactionRequest
        (
            Amount: 100,
            Type: (TransactionType)100 // Invalid enum
        );
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var validator = CreateValidator();
        var model = new CreateTransactionRequest
        (
            Amount: 100,
            Type: TransactionType.Deposit,
            Description: "Valid"
        );
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}