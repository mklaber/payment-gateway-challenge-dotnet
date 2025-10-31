using MediatR;

using PaymentGateway.Core.Models;
using PaymentGateway.Core.Repositories;

namespace PaymentGateway.Core.Commands;

public record AddPaymentRequest(Payment Payment) : IRequest<bool>;

public class AddPaymentRequestHandler(IPaymentsRepository repo) : IRequestHandler<AddPaymentRequest, bool>
{
    public Task<bool> Handle(AddPaymentRequest request, CancellationToken cancellationToken)
    {
        repo.Add(request.Payment);
        // do we care about this value? nah. But MediatR behaviours may not trigger for requests
        // that do not have responses so it's a good practice to just return... something.
        return Task.FromResult(true);
    }
}