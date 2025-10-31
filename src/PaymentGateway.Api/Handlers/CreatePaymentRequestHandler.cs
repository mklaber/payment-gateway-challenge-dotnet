using FluentValidation;

using MapsterMapper;

using MediatR;

using PaymentGateway.Api.Clients.Mountebank;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Core.Commands;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Repositories;

namespace PaymentGateway.Api.Handlers;

public class CreatePaymentRequestHandler(
    IValidator<CreatePaymentRequest> createPaymentValidator,
    IMapper mapper,
    IMediator mediator,
    IMountebankClient client) : IRequestHandler<CreatePaymentRequest, SuccessOrFailure<PaymentDto>>
{

    public async Task<SuccessOrFailure<PaymentDto>> Handle(CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        // NB: this belongs in an open behaviour
        var validationResult = await createPaymentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new(null, validationResult.ToDictionary());
        }

        var createPaymentReq = mapper.Map<PaymentRequestExternalDto>(request);
        // this will throw... do we want to catch and be more precise?
        var response = await client.CreatePaymentAsync(createPaymentReq, cancellationToken);
        var payment = mapper.Map<Payment>((request, response));
        
        await mediator.Send(new AddPaymentRequest(payment), cancellationToken);
        
        var dto = mapper.Map<PaymentDto>(payment);
        
        return new SuccessOrFailure<PaymentDto>(dto);
    }
}