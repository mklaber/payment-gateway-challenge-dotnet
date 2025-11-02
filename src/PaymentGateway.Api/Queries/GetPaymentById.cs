using MediatR;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Queries;

public record GetPaymentByIdRequest(Guid PaymentId) : IRequest<Payment?>;

public class GetPaymentByIdRequestHandler(IPaymentsRepository repo) : IRequestHandler<GetPaymentByIdRequest, Payment?>
{
    public Task<Payment?> Handle(GetPaymentByIdRequest request, CancellationToken cancellationToken)
        => Task.FromResult(repo.GetById(request.PaymentId));
}