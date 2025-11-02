using PaymentGateway.Core.Commands;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Repositories;

namespace PaymentGateway.Core.Tests.Commands;

public class AddPaymentTests
{
    [Fact]
    public async Task Handle_Works()
    {
        // Arrange
        var pay = new Payment() { PaymentId = Guid.NewGuid() };
        var request = new AddPaymentRequest(pay);
        var repo = Substitute.For<IPaymentsRepository>();
        var sut = new AddPaymentRequestHandler(repo);

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        repo.Received(1).Add(Arg.Is<Payment>(x => x.PaymentId == pay.PaymentId));
    }
}