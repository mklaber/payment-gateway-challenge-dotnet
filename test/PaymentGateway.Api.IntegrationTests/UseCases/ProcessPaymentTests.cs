using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using PaymentGateway.Api.Contracts;

using Xunit.Abstractions;

namespace PaymentGateway.Api.IntegrationTests.UseCases;

public class ProcessPaymentTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;
    private readonly Random _random = new();

    public ProcessPaymentTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _client = factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(s =>
            {
                TestAuthHandler.ConfigureTestServices(s);
            });
        }).CreateClient();
        TestAuthHandler.ConfigureHttpClient(_client);
    }

    [Fact]
    public async Task Creates_Payment_Fails_For_Upstream_Error()
    {
        // Arrange
        CreatePaymentRequest payment = new()
        {
            ExpiryYear = _random.Next(DateTime.Now.Year + 1, DateTime.Now.Year + 25),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumber = "4111111111111110",
            Currency = "GBP",
            Cvv = $"{_random.Next(100, 999)}"
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/payments", payment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        string raw = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(raw);
        raw.Should().NotContain(payment.CardNumber, "do not leak card number");
        ProblemDetails? paymentFailure =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonHelpers.StandardOptions);
        paymentFailure.Should().NotBeNull();
    }

    /// <summary>
    ///     We do not need to test every validation failure possibility; in this integration test all we're doing is
    ///     confirming that validators are wired up correctly and returning what we expect them to return.
    /// </summary>
    [Fact]
    public async Task Create_Payment_Rejects()
    {
        CreatePaymentRequest payment = new()
        {
            ExpiryYear = DateTime.Now.Year - 1,
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumber = "1234",
            Currency = "GBP",
            Cvv = $"{_random.Next(100, 999)}"
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/payments", payment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
        ValidationProblemDetails? paymentFailure =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonHelpers.StandardOptions);
        paymentFailure.Should().NotBeNull();
        paymentFailure.Errors.Should().NotBeNullOrEmpty();
        paymentFailure.Errors.Should().ContainKey("CardNumber").WhoseValue.Should().ContainSingle().Which.Should()
            .Contain("must be at least 14 characters");
        paymentFailure.Errors.Should().ContainKey("", "because it is an overall error").WhoseValue.Should()
            .ContainSingle().Which.Should().Contain("must be in the future");
    }

    [Theory]
    [InlineData("4111111111111111", PaymentStatusDto.Authorized)]
    [InlineData("4111111111111112", PaymentStatusDto.Declined)]
    public async Task Creates_Payment_Successfully_and_Retrieves_it(string ccNumber, PaymentStatusDto paymentStatus)
    {
        // Arrange
        // HttpClient client = factory.CreateClient();
        CreatePaymentRequest payment = new()
        {
            ExpiryYear = _random.Next(DateTime.Now.Year + 1, DateTime.Now.Year + 25),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumber = ccNumber,
            Currency = "GBP",
            Cvv = $"{_random.Next(100, 999)}"
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/payments", payment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
        PaymentDto? createdPayment = await response.Content.ReadFromJsonAsync<PaymentDto>(JsonHelpers.StandardOptions);
        createdPayment.Should().NotBeNull();
        createdPayment.PaymentId.Should().NotBeEmpty();
        createdPayment.Status.Should().Be(paymentStatus);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().Be($"/payments/{createdPayment.PaymentId}");

        // Re-Act
        var retrievedPaymentResponse = await _client.GetAsync($"/payments/{createdPayment.PaymentId}");
        retrievedPaymentResponse.Should().NotBeNull();
        retrievedPaymentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        PaymentDto? retrievedPayment =
            await retrievedPaymentResponse.Content.ReadFromJsonAsync<PaymentDto>(JsonHelpers.StandardOptions);
        retrievedPayment.Should().NotBeNull();
        retrievedPayment.Should().BeEquivalentTo(createdPayment);
    }
}