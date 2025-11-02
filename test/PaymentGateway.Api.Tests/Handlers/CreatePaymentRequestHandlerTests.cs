using FluentResults;

using FluentValidation;
using FluentValidation.Results;

using MapsterMapper;

using MediatR;

using PaymentGateway.Api.Clients.Mountebank;
using PaymentGateway.Api.Commands;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Handlers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Results;

namespace PaymentGateway.Api.Tests.Handlers;

public class CreatePaymentRequestHandlerTests
{
    private readonly IValidator<CreatePaymentRequest> _validator;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IMountebankClient _client;
    private readonly CreatePaymentRequestHandler _sut;

    public CreatePaymentRequestHandlerTests()
    {
        _validator = Substitute.For<IValidator<CreatePaymentRequest>>();
        _mapper = Substitute.For<IMapper>();

        // _mapper.Map<PaymentRequestExternalDto>(Arg.Any<CreatePaymentRequest>()).Returns(new PaymentRequestExternalDto());
        _mediator = Substitute.For<IMediator>();
        _client = Substitute.For<IMountebankClient>();
        // _client.CreatePaymentAsync(Arg.Any<PaymentRequestExternalDto>(), Arg.Any<CancellationToken>())
        //     .Returns(Result.Ok(new PaymentResponseExternalDto(true, "some-code")));
        // _mapper.Map<Payment>(Arg.Any<Tuple<CreatePaymentRequest, PaymentResponseExternalDto, PaymentRequestExternalDto>>())
        //     .Returns(new Payment());
        // _mapper.Map<PaymentDto>(Arg.Any<Payment>())
        //     .Returns(new PaymentDto());
        _sut = new CreatePaymentRequestHandler(_validator, _mapper, _mediator, _client);
    }

    [Fact]
    public async Task Handle_WhenClientCreatePaymentFails_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreatePaymentRequest();
        var externalDto = new PaymentRequestExternalDto();
        var clientResponse = Result.Fail(new BankError(true));
        _validator.ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _mapper.Map<PaymentRequestExternalDto>(request)
            .Returns(externalDto);
        _client.CreatePaymentAsync(externalDto, Arg.Any<CancellationToken>())
            .Returns(clientResponse);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.HasError<BankError>().Should().BeTrue();
        var be = result.Errors.Should().ContainSingle().Which.Should().BeOfType<BankError>().Which;
        be.IsTransient.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationSucceeds_And_ClientSucceeds_ReturnsSuccessResult()
    {
        // Arrange
        var request = new CreatePaymentRequest();
        var externalDto = new PaymentRequestExternalDto();
        var response = Result.Ok(new PaymentResponseExternalDto(true, "some-string"));
        var payment = new Payment();
        var paymentDto = new PaymentDto();

        _validator.ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _mapper.Map<PaymentRequestExternalDto>(request)
            .Returns(externalDto);
        _client.CreatePaymentAsync(externalDto, Arg.Any<CancellationToken>())
            .Returns(response);
        _mapper.Map<Payment>(Arg.Any<(CreatePaymentRequest, PaymentResponseExternalDto)>())
            .Returns(payment);
        _mapper.Map<PaymentDto>(payment)
            .Returns(paymentDto);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(paymentDto);

        await _validator.Received(1).ValidateAsync(request, Arg.Any<CancellationToken>());
        _mapper.Received(1).Map<PaymentRequestExternalDto>(request);
        await _client.Received(1).CreatePaymentAsync(externalDto, Arg.Any<CancellationToken>());
        await _mediator.Received(1)
            .Send(Arg.Is<AddPaymentRequest>(r => r.Payment == payment), Arg.Any<CancellationToken>());
        _mapper.Received(1).Map<PaymentDto>(payment);
    }


    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var request = new CreatePaymentRequest();
        var validationFailures = new List<ValidationFailure> { new("PropertyName", "Error Message") };
        var validationResult = new ValidationResult(validationFailures);

        _validator.ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(validationResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailed.Should().BeTrue();
        result.HasError<ValidationError>().Should().BeTrue();
        var ve = result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>().Which;
        ve.ValidationResult.Errors.Should().HaveCount(1).And.ContainSingle().Which.PropertyName.Should()
            .Be("PropertyName");

        await _validator.Received(1).ValidateAsync(request, Arg.Any<CancellationToken>());
        _mapper.DidNotReceive().Map<PaymentRequestExternalDto>(Arg.Any<CreatePaymentRequest>());
        await _client.DidNotReceiveWithAnyArgs()
            .CreatePaymentAsync(Arg.Any<PaymentRequestExternalDto>(), Arg.Any<CancellationToken>());
        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<IRequest>(), Arg.Any<CancellationToken>());
        _mapper.ReceivedCalls().Should().BeEmpty("mapper should not have been called");
    }
}