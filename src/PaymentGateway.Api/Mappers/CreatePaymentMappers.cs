using Mapster;

using PaymentGateway.Api.Clients.Mountebank;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Utilities;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Api.Mappers;

/// <summary>
/// Mappers involved in the creation of a payment
/// </summary>
public class CreatePaymentMappers : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreatePaymentRequest, PaymentRequestExternalDto>()
            .Map(dest => dest.ExpiryDate,
                src => src.ToExpiryDate().ToString("MM/yyyy"));

        // There are lots of uses of "!" and other mappings of nullable to non-nullable things. That's okay because
        // we'll only use this mapper after we've validated the inputs (we're not storing a Payment if it's rejected
        // for validation reasons)
        config.NewConfig<(CreatePaymentRequest CreateCommand, PaymentResponseExternalDto BankResponse), Payment>()
            .Map(dest => dest.PaymentId, src => Guid.NewGuid())
            // NB: there are more succinct ways to do this (e.g., [^4...]) but Mapster requires expression trees which limit our options
            .Map(dest => dest.CardNumberLastFour,
                src => int.Parse(src.CreateCommand.CardNumber!.Substring(src.CreateCommand.CardNumber.Length - 4, 4)))
            .Map(dest => dest.Status,
                src => src.BankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined)
            .Map(dest => dest.BankAuthorizationCode,
                src => string.IsNullOrEmpty(src.BankResponse.AuthorizationCode)
                    ? null
                    : src.BankResponse.AuthorizationCode)
            // these are maps that seem like they should work implicitly, but... reminder: we're mapping from a tuple!
            .Map(dest => dest.ExpiryYear, src => src.CreateCommand.ExpiryYear)
            .Map(dest => dest.ExpiryMonth, src => src.CreateCommand.ExpiryMonth)
            .Map(dest => dest.Currency, src => src.CreateCommand.Currency)
            .Map(dest => dest.Amount, src => src.CreateCommand.Amount);
    }
}