using MapsterMapper;

using MediatR;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Handlers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Queries;

namespace PaymentGateway.Api.Tests.Handlers;

public class GetPaymentRequestHandlerTests
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly GetPaymentRequestHandler _sut;

    public GetPaymentRequestHandlerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _mapper = Substitute.For<IMapper>();
        _sut = new GetPaymentRequestHandler(_mediator, _mapper);
    }

    [Fact]
    public async Task Handle_WhenPaymentExists_ReturnsPaymentDto()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var request = new GetPaymentRequest(paymentId);
        var payment = new Payment();
        var paymentDto = new PaymentDto();

        _mediator.Send(Arg.Is<GetPaymentByIdRequest>(r => r.PaymentId == paymentId), Arg.Any<CancellationToken>())
            .Returns(payment);
        _mapper.Map<PaymentDto>(payment)
            .Returns(paymentDto);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(paymentDto);

        await _mediator.Received(1).Send(Arg.Is<GetPaymentByIdRequest>(r => r.PaymentId == paymentId),
            Arg.Any<CancellationToken>());
        _mapper.Received(1).Map<PaymentDto>(payment);
    }

    [Fact]
    public async Task Handle_WhenPaymentDoesNotExist_ReturnsNull()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var request = new GetPaymentRequest(paymentId);

        _mediator.Send(Arg.Is<GetPaymentByIdRequest>(r => r.PaymentId == paymentId), Arg.Any<CancellationToken>())
            .Returns((Payment?)null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await _mediator.Received(1).Send(Arg.Is<GetPaymentByIdRequest>(r => r.PaymentId == paymentId),
            Arg.Any<CancellationToken>());
        _mapper.DidNotReceive().Map<PaymentDto>(Arg.Any<Payment>());
    }
}