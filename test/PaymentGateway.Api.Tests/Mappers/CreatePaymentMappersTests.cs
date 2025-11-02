using PaymentGateway.Api.Clients.Mountebank;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Mappers;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Api.Tests.Mappers;

public class CreatePaymentMappersTests() : MapperTests(() => new CreatePaymentMappers())
{
    [Fact]
    public void CreatePaymentRequest_Maps_To_PaymentRequestExternalDto_Correctly()
    {
        // Arrange
        var request = new CreatePaymentRequest { ExpiryMonth = 2, ExpiryYear = 2025 };

        // Act
        var result = Sut.Map<PaymentRequestExternalDto>(request);

        // Assert
        result.ExpiryDate.Should().Be("02/2025");
        // we're not testing the rest of the properties because they're implicitly mapped
        // and would instead be picked up by Mapping_Is_Valid
    }


    [Fact]
    public void Tuple_Maps_To_Payment_Correctly_When_Payment_Authorized()
    {
        // Arrange
        var createRequest = new CreatePaymentRequest
        {
            CardNumber = "4532999999991234",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 10050
        };

        var bankResponse = new PaymentResponseExternalDto(true, "AUTH123");

        var tuple = (createRequest, bankResponse);

        // Act
        var result = Sut.Map<Payment>(tuple);

        // Assert
        result.Should().NotBeNull();
        result.PaymentId.Should().NotBeEmpty();
        result.CardNumberLastFour.Should().Be(1234);
        result.Status.Should().Be(PaymentStatus.Authorized);
        result.BankAuthorizationCode.Should().Be("AUTH123");
        result.ExpiryYear.Should().Be(2025);
        result.ExpiryMonth.Should().Be(12);
        result.Currency.Should().Be("USD");
        result.Amount.Should().Be(10050);
    }


    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Tuple_Maps_To_Payment_Correctly_When_Payment_Declined(string? authCode)
    {
        // Arrange
        var createRequest = new CreatePaymentRequest
        {
            CardNumber = "4532999999991234",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 10050
        };

        var bankResponse = new PaymentResponseExternalDto(false, authCode);

        var tuple = (createRequest, bankResponse);

        // Act
        var result = Sut.Map<Payment>(tuple);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentStatus.Declined);
        result.BankAuthorizationCode.Should().BeNull();
    }
}