using System.Net.Mime;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Contracts;

namespace PaymentGateway.Api.Controllers;

[Route("payments")]
[ApiController]
[Authorize]
public class PaymentsController(IMediator mediator) : Controller
{
    /// <summary>
    /// Get a payment by ID
    /// </summary>
    /// <param name="paymentId">A unique ID, provided by this service</param>
    /// <returns>The previously submitted payment</returns>
    /// <response code="200">A previously submitted payment</response>
    /// <response code="404">If no payment is found for the given ID and merchant</response>
    /// <remarks>
    /// Merchants can retrieve the details of a payment that was successfully created -- whether its `status` is `Authorized`
    /// or `Rejected`.
    /// </remarks>
    [HttpGet("{paymentId:guid}", Name = nameof(GetPaymentById))]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto?>> GetPaymentById(Guid paymentId)
    {
        var payment = await mediator.Send(new GetPaymentRequest(paymentId));

        if (payment == null)
        {
            return NotFound();
        }

        return Ok(payment);
    }

    /// <summary>
    /// Create a payment
    /// </summary>
    /// <param name="paymentRequest">The details of a payment</param>
    /// <returns>The created payment</returns>
    /// <response code="201">The newly created payment. A `Location` header will also be set.</response>
    /// <response code="422">The provided payment does not meet our strict validation rules. Detailed problems of the
    /// request are provided in the `errors` property. These attempt to be as specific as possible about the underlying
    /// issues with the request. Requests of this type should not be retried without modifications to the request.</response>
    /// <response code="400">No request body was provided.</response>
    /// <remarks>
    /// This API provides Merchants the ability to attempt to take money from customers.
    ///
    /// Successful requests to this endpoint do not necessarily mean successful payments. We'll return a `status` of
    /// `Authorized` or `Declined` if the request appears to be valid but the card does not have funds available or
    /// otherwise fails to authorize a money transfer.
    /// 
    /// Sample request:
    ///
    ///     POST /payments
    ///     {
    ///       "cardNumber": "4111111111111111",
    ///       "expiryMonth": 12,
    ///       "expiryYear": 2027,
    ///       "currency": "EUR",
    ///       "amount": 1123,
    ///       "cvv": "123"
    ///     }
    ///
    /// Will yield a response body and a `Location` header:
    ///
    ///     Location: http.../payment/9904c443-c1da-41c2-8301-bccb73cc519f
    ///     {
    ///       "paymentId": "9904c443-c1da-41c2-8301-bccb73cc519f",
    ///       "status": "Authorized",
    ///       "cardNumberLastFour": 1111,
    ///       "expiryMonth": 12,
    ///       "expiryYear": 2027,
    ///       "currency": "EUR",
    ///       "amount": 1123
    ///     }
    ///
    /// If your HTTP Client automatically follows redirects, it'll make a subsequent call to the `GET` endpoint.
    /// </remarks>
    [HttpPost(Name = nameof(CreatePayment))]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> CreatePayment(CreatePaymentRequest paymentRequest)
    {
        var paymentResult = await mediator.Send(paymentRequest);
        if (paymentResult.Failures is not null)
        {
            return UnprocessableEntity(new ValidationProblemDetails(paymentResult.Failures));
        }

        return CreatedAtRoute(nameof(GetPaymentById), new { paymentId = paymentResult.Success!.PaymentId },
            paymentResult.Success);
    }
}