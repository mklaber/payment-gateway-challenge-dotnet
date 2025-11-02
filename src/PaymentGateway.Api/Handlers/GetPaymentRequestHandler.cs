using MapsterMapper;

using MediatR;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Queries;

namespace PaymentGateway.Api.Handlers;

public class GetPaymentRequestHandler(IMediator mediator, IMapper mapper)
    : IRequestHandler<GetPaymentRequest, PaymentDto?>
{
    public async Task<PaymentDto?> Handle(GetPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await mediator.Send(new GetPaymentByIdRequest(request.PaymentId), cancellationToken);
        if (payment is null) return null;
        return mapper.Map<PaymentDto>(payment);
    }
}