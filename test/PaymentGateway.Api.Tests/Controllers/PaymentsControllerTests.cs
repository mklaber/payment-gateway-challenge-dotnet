using MediatR;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Controllers;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly IMediator _mediator;
    private readonly PaymentsController _sut;

    public PaymentsControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _sut = new PaymentsController(_mediator);
    }

    [Fact]
    public async Task GetPaymentById_WhenPaymentExists_ReturnsOkWithPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentDto = new PaymentDto { PaymentId = paymentId };
        _mediator.Send(Arg.Is<GetPaymentRequest>(r => r.PaymentId == paymentId), Arg.Any<CancellationToken>())
            .Returns(paymentDto);

        // Act
        var result = await _sut.GetPaymentById(paymentId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPayment = okResult.Value.Should().BeOfType<PaymentDto>().Subject;
        returnedPayment.Should().Be(paymentDto);

        await _mediator.Received(1).Send(Arg.Is<GetPaymentRequest>(r => r.PaymentId == paymentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPaymentById_WhenPaymentDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _mediator.Send(Arg.Is<GetPaymentRequest>(r => r.PaymentId == paymentId), Arg.Any<CancellationToken>())
            .Returns((PaymentDto?)null);

        // Act
        var result = await _sut.GetPaymentById(paymentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        await _mediator.Received(1).Send(Arg.Is<GetPaymentRequest>(r => r.PaymentId == paymentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePayment_WhenValid_ReturnsCreatedWithPayment()
    {
        // Arrange
        var request = new CreatePaymentRequest();
        var payment = new PaymentDto { PaymentId = Guid.NewGuid() };
        var result = new SuccessOrFailure<PaymentDto>(payment);

        _mediator.Send(request, Arg.Any<CancellationToken>()).Returns(result);

        // Act
        var actionResult = await _sut.CreatePayment(request);

        // Assert
        var createdResult = actionResult.Should().BeOfType<CreatedAtRouteResult>().Subject;
        createdResult.RouteName.Should().Be(nameof(PaymentsController.GetPaymentById));
        createdResult.RouteValues!["id"].Should().Be(payment.PaymentId);
        createdResult.Value.Should().Be(payment);

        await _mediator.Received(1).Send(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePayment_WhenInvalid_ReturnsUnprocessableEntity()
    {
        // Arrange
        var request = new CreatePaymentRequest();
        var failures = new Dictionary<string, string[]> { { "PropertyName", new[] { "Error message" } } };
        var result = new SuccessOrFailure<PaymentDto>(null, failures);

        _mediator.Send(request, Arg.Any<CancellationToken>()).Returns(result);

        // Act
        var actionResult = await _sut.CreatePayment(request);

        // Assert
        var unprocessableResult = actionResult.Should().BeOfType<UnprocessableEntityObjectResult>().Subject;
        var problemDetails = unprocessableResult.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problemDetails.Errors.Should().ContainKey("PropertyName");

        await _mediator.Received(1).Send(request, Arg.Any<CancellationToken>());
    }
}