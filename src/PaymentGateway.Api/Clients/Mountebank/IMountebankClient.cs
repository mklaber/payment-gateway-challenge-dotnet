namespace PaymentGateway.Api.Clients.Mountebank;

public interface IMountebankClient
{
    Task<PaymentResponseExternalDto> CreatePaymentAsync(PaymentRequestExternalDto paymentRequestDto,
        CancellationToken cancellationToken);
}