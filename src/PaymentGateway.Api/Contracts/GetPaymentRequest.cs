using MediatR;

namespace PaymentGateway.Api.Contracts;

public record GetPaymentRequest(Guid PaymentId) : IRequest<PaymentDto?>;