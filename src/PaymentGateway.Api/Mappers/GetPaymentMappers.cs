using Mapster;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Api.Mappers;


/// <summary>
/// Mappers involved in getting existing payments
/// </summary>
public class GetPaymentMappers : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Payment, PaymentDto>();
    }
}