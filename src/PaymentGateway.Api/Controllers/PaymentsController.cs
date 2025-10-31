using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Contracts;

namespace PaymentGateway.Api.Controllers;

[Route("payments")]
[ApiController]
public class PaymentsController(IMediator mediator) : Controller
{
    // private readonly PaymentsRepository _paymentsRepository;
    //
    // public PaymentsController(PaymentsRepository paymentsRepository)
    // {
    //     _paymentsRepository = paymentsRepository;
    // }

    /// <summary>
    /// Gets a previously submitted payment, by unique ID
    /// </summary>
    /// <param name="id">A unique ID, provided by this service</param>
    /// <returns>The previously submitted payment</returns>
    [HttpGet("{id:guid}", Name = nameof(GetPaymentById))]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto?>> GetPaymentById(Guid id)
    {
        var payment = await mediator.Send(new GetPaymentRequest(id));

        if (payment == null)
        {
            return NotFound();
        }

        return Ok(payment);
    }

    /// <summary>
    /// Creates a new payment
    /// </summary>
    /// <param name="paymentRequest"></param>
    /// <returns></returns>
    [HttpPost(Name = nameof(CreatePayment))]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>( StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreatePayment(CreatePaymentRequest paymentRequest)
    {
        var paymentResult = await mediator.Send(paymentRequest);
        if (paymentResult.Failures is not null)
        {
            return UnprocessableEntity(new ValidationProblemDetails(paymentResult.Failures));
            // ModelState.
            // return ValidationProblem();
            // return Results.ValidationProblem(paymentResult.Failures);
        }

        return CreatedAtRoute(nameof(GetPaymentById), new { id = paymentResult.Success!.PaymentId },
            paymentResult.Success);
    }
}