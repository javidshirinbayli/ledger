using FluentValidation;
using Ledger.Core.Models.Dtos;
using Microsoft.Extensions.Options;

namespace Ledger.Core.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator(IOptions<TransactionLimitsOptions> options)
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(options.Value.MaxAmount)
            .WithMessage($"Amount cannot exceed {options.Value.MaxAmount}");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid transaction type");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}