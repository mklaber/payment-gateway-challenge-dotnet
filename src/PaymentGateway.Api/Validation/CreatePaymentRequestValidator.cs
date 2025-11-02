using FluentValidation;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Utilities;

namespace PaymentGateway.Api.Validation;

/// <summary>
/// Defines validation rules for the <see cref="CreatePaymentRequest"/>
/// </summary>
/// <remarks>It is really important to have thorough and descriptive validation rules so that
/// we can make it abundantly clear to the merchant why their call has failed and what they can do
/// to fix it.</remarks>
public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    private static readonly string[] SupportedCurrencies = ["CHF", "EUR", "GBP"];

    public CreatePaymentRequestValidator()
    {
        RuleFor(c => c.CardNumber)
            .NotEmpty()
            .MinimumLength(14)
            .MaximumLength(19)
            .NumericOnly();

        RuleFor(c => c.ExpiryMonth)
            .NotEmpty()
            .InclusiveBetween(1, 12);

        RuleFor(c => c.ExpiryYear)
            // ExpiryYear being a valid, future, year is handled in a custom rule below
            .NotEmpty();

        RuleFor(c => c)
            .Custom((c, context) =>
            {
                // do cc expire on the first day of the expiry month/year? 
                if (!c.TryToExpiryDate(out DateOnly expiryDt) || DateTime.Now.ToExpiryDate() >= expiryDt)
                {
                    context.AddFailure("Expiry date and month must be in the future.");
                }
            });

        RuleFor(c => c.Currency)
            .NotEmpty();

        RuleFor(c => c.Currency)
            .Must(curr => SupportedCurrencies.Contains(curr))
            .WithMessage(
                $"'{{PropertyValue}}' is not a supported currency. It must be one of: {string.Join(", ", SupportedCurrencies)}")
            .Unless(c => string.IsNullOrEmpty(c.Currency));

        RuleFor(c => c.Amount)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(c => c.Cvv)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(4)
            .NumericOnly();
    }
}