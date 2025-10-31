using FluentValidation.TestHelper;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Tests.Validation;

public class CreatePaymentRequestValidatorTests
{
    private readonly CreatePaymentRequestValidator _sut = new();

    [Fact]
    public void CreatePaymentRequest_Is_Valid()
    {
        // Arrange
        var request = CreateValidRequest();
        
        // Act
        var result = _sut.TestValidate(request);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Properties_Are_Required()
    {
        // Arrange
        var request = new CreatePaymentRequest();
        
        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CardNumber).MatchedFailures.Should()
            .ContainSingle(f => f.PropertyName == nameof(CreatePaymentRequest.CardNumber)).Which.ErrorCode.Should()
            .Be("NotEmptyValidator", "we need to verify that this is _why_ it is failing");
        result.ShouldHaveValidationErrorFor(c => c.ExpiryMonth).MatchedFailures.Should()
            .ContainSingle(f => f.PropertyName == nameof(CreatePaymentRequest.ExpiryMonth)).Which.ErrorCode.Should()
            .Be("NotEmptyValidator", "we need to verify that this is _why_ it is failing");
        result.ShouldHaveValidationErrorFor(c => c.ExpiryYear).MatchedFailures.Should()
            .ContainSingle(f => f.PropertyName == nameof(CreatePaymentRequest.ExpiryYear)).Which.ErrorCode.Should()
            .Be("NotEmptyValidator", "we need to verify that this is _why_ it is failing");
        result.ShouldHaveValidationErrorFor(c => c.Currency).MatchedFailures.Should()
            .ContainSingle(f => f.PropertyName == nameof(CreatePaymentRequest.Currency)).Which.ErrorCode.Should()
            .Be("NotEmptyValidator", "we need to verify that this is _why_ it is failing");
        result.ShouldHaveValidationErrorFor(c => c.Amount).MatchedFailures.Should()
            .ContainSingle(f => f.PropertyName == nameof(CreatePaymentRequest.Amount)).Which.ErrorCode.Should()
            .Be("NotEmptyValidator", "we need to verify that this is _why_ it is failing");
        result.ShouldHaveValidationErrorFor(c => c.Cvv).MatchedFailures.Should()
            .ContainSingle(f => f.PropertyName == nameof(CreatePaymentRequest.Cvv)).Which.ErrorCode.Should()
            .Be("NotEmptyValidator", "we need to verify that this is _why_ it is failing");


    }
    
    [Theory]
    [InlineData(null)] // not empty
    [InlineData("")] // not empty
    [InlineData("123")]  // Too short
    [InlineData("12345678901234567890")] // Too long
    [InlineData("4abcd11111111111")]  // Non-numeric
    public void CardNumber_ShouldBeInvalid_WhenNotMatchingRules(string? cardNumber)
    {
        // Arrange
        var request = CreateValidRequest(cardNumber: cardNumber);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void ExpiryMonth_ShouldBeInvalid_WhenOutsideValidRange(int month)
    {
        // Arrange
        var request = CreateValidRequest(expiryMonth: month);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth);
    }


    [Fact]
    public void ExpiryDate_ShouldBeInvalid_WhenThisMonth()
    {
        // Arrange
        var request = CreateValidRequest(expiryMonth: DateTime.Now.Month, expiryYear: DateTime.Now.Year);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        var badExpiryValidation = result.ShouldHaveValidationErrors().MatchedFailures.Should().ContainSingle().Which;
        badExpiryValidation.ErrorMessage.Should().Be("Expiry date and month must be in the future.");
    }
    
    [Fact]
    public void ExpiryDate_ShouldBeInvalid_WhenInPast()
    {
        // Arrange
        var pastDate = DateTime.Now.AddMonths(-1);
        var request = CreateValidRequest(expiryMonth: DateTime.Now.Month, expiryYear: DateTime.Now.Year);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        var badExpiryValidation = result.ShouldHaveValidationErrors().MatchedFailures.Should().ContainSingle().Which;
        badExpiryValidation.ErrorMessage.Should().Be("Expiry date and month must be in the future.");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("JPY")]
    public void Currency_ShouldBeInvalid_WhenNotSupported(string currency)
    {
        // Arrange
        var request = CreateValidRequest(currency: currency);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        var currencyFailure = result.ShouldHaveValidationErrorFor(x => x.Currency).MatchedFailures.Should()
            .ContainSingle().Which;
        currencyFailure.ErrorMessage.Should().Be($"'{currency}' is not a supported currency. It must be one of: CHF, EUR, GBP");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Amount_ShouldBeInvalid_WhenNotPositive(int amount)
    {
        // Arrange
        var request = CreateValidRequest(amount: amount);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]  // Too short
    [InlineData("12345")]  // Too long
    [InlineData("12a")]  // Non-numeric
    public void Cvv_ShouldBeInvalid_WhenNotMatchingRules(string cvv)
    {
        // Arrange
        var request = CreateValidRequest(cvv: cvv);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cvv);
    }

    private static CreatePaymentRequest CreateValidRequest(string? cardNumber = "4111111111111111", int? expiryMonth = 2, int? expiryYear = 2050, string? currency = "EUR", int? amount = 10050, string? cvv = "123") => new()
    {
        CardNumber = cardNumber,
        ExpiryMonth = expiryMonth,
        ExpiryYear = expiryYear,
        Currency = currency,
        Amount = amount,
        Cvv = cvv
    };
}