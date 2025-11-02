using PaymentGateway.Api.Models;
using PaymentGateway.Api.Queries;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Tests.Queries;

public class GetPaymentByTests
{
    [Fact]
    public async Task Handle_Works()
    {
        // Arrange
        var pay = new Payment() { PaymentId = Guid.NewGuid() };
        var request = new GetPaymentByIdRequest(pay.PaymentId);
        var repo = Substitute.For<IPaymentsRepository>();
        repo.GetById(pay.PaymentId).Returns(pay);
        var sut = new GetPaymentByIdRequestHandler(repo);

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PaymentId.Should().Be(pay.PaymentId);
        repo.Received(1).GetById(pay.PaymentId);
    }
}