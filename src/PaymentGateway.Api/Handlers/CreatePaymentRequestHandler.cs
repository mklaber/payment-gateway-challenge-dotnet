using FluentResults;

using FluentValidation;

using MapsterMapper;

using MediatR;

using PaymentGateway.Api.Clients.Mountebank;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Results;
using PaymentGateway.Core.Commands;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Api.Handlers;

public class CreatePaymentRequestHandler(
    IValidator<CreatePaymentRequest> createPaymentValidator,
    IMapper mapper,
    IMediator mediator,
    IMountebankClient client) : IRequestHandler<CreatePaymentRequest, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        // NB: this belongs in an open behaviour
        var validationResult = await createPaymentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Fail(new ValidationError(validationResult));
        }

        var createPaymentReq = mapper.Map<PaymentRequestExternalDto>(request);

        var response = await client.CreatePaymentAsync(createPaymentReq, cancellationToken);
        if (response.IsFailed)
        {
            return response.ToResult<PaymentDto>();
        }

        var payment = mapper.Map<Payment>((request, response.Value));

        await mediator.Send(new AddPaymentRequest(payment), cancellationToken);

        var dto = mapper.Map<PaymentDto>(payment);

        return Result.Ok(dto);
    }
}