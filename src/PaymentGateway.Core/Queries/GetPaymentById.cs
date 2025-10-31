using MediatR;

using PaymentGateway.Core.Models;
using PaymentGateway.Core.Repositories;

namespace PaymentGateway.Core.Queries;

public record GetPaymentByIdRequest(Guid PaymentId) : IRequest<Payment?>;

public class GetPaymentByIdRequestHandler(IPaymentsRepository repo) : IRequestHandler<GetPaymentByIdRequest, Payment?>
{
    public Task<Payment?> Handle(GetPaymentByIdRequest request, CancellationToken cancellationToken)
        => Task.FromResult(repo.GetById(request.PaymentId));
}