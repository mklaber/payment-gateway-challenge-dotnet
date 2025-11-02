using FluentResults;

using FluentValidation.Results;

namespace PaymentGateway.Api.Results;

public class ValidationError : Error
{
    public ValidationResult ValidationResult { get; init; }

    public ValidationError(ValidationResult result) : base("A validation error occured.")
    {
        ValidationResult = result;
        WithMetadata("details", result.ToDictionary());
    }
}