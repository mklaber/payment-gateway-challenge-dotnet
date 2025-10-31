using System.Diagnostics.CodeAnalysis;

using FluentValidation;

namespace PaymentGateway.Api.Validation;

[ExcludeFromCodeCoverage]
public static class CustomValidators
{
    /// <summary>
    /// Defines a validator that will fail if the given string contains any characters but the digits 0 to 9
    /// </summary>
    public static IRuleBuilderOptions<T, string?> NumericOnly<T>(this IRuleBuilder<T, string?> ruleBuilder)
        => ruleBuilder.Matches(@"^\d*$").WithMessage("'{PropertyName}' must contain only numbers");
}