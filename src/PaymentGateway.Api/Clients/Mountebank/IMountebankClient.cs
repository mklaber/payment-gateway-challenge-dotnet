using FluentResults;

namespace PaymentGateway.Api.Clients.Mountebank;

public interface IMountebankClient
{
    Task<Result<PaymentResponseExternalDto>> CreatePaymentAsync(PaymentRequestExternalDto paymentRequestDto,
        CancellationToken cancellationToken);
}