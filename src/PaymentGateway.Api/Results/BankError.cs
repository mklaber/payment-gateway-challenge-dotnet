using FluentResults;

namespace PaymentGateway.Api.Results;

public class BankError : Error
{
    public bool IsTransient { get; init; }

    public BankError(bool isTransient) : base("PaymentGatewayApi::BankError")
    {
        IsTransient = isTransient;
        WithMetadata("isTransient", isTransient);
    }
}