using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

using PaymentGateway.Api.Contracts;

using Xunit.Abstractions;

namespace PaymentGateway.Api.IntegrationTests.UseCases;

public class ProcessPaymentTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Random _random = new();

    [Fact]
    public async Task Creates_Payment_Fails_For_Upstream_Error()
    {
        // Arrange
        HttpClient client = factory.CreateClient();
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
        HttpResponseMessage response = await client.PostAsJsonAsync("/payments", payment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        string raw = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine(raw);
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
        // Arrange
        HttpClient client = factory.CreateClient();
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
        HttpResponseMessage response = await client.PostAsJsonAsync("/payments", payment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
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
    public async Task Creates_Payment_Successfully(string ccNumber, PaymentStatusDto paymentStatus)
    {
        // Arrange
        HttpClient client = factory.CreateClient();
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
        HttpResponseMessage response = await client.PostAsJsonAsync("/payments", payment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
        PaymentDto? createdPayment = await response.Content.ReadFromJsonAsync<PaymentDto>(JsonHelpers.StandardOptions);
        createdPayment.Should().NotBeNull();
        createdPayment.PaymentId.Should().NotBeEmpty();
        createdPayment.Status.Should().Be(paymentStatus);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath.Should().Be($"/payments/{createdPayment.PaymentId}");
    }
    // private readonly Random _random = new();
    //
    // [Fact(Skip = "skip for now")]
    // public async Task RetrievesAPaymentSuccessfully()
    // {
    //     // Arrange
    //     var payment = new Payment
    //     {
    //         PaymentId = Guid.NewGuid(),
    //         ExpiryYear = _random.Next(2023, 2030),
    //         ExpiryMonth = _random.Next(1, 12),
    //         Amount = _random.Next(1, 10000),
    //         CardNumberLastFour = _random.Next(1111, 9999),
    //         Currency = "GBP"
    //     };
    //     
    //     // add a test that gives a valid currency code but that we don't support
    //
    //     var paymentsRepository = new PaymentsRepository();
    //     paymentsRepository.Add(payment);
    //
    //     var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
    //     var client = webApplicationFactory.WithWebHostBuilder(builder =>
    //             builder.ConfigureServices(services => ((ServiceCollection)services)
    //                 .AddSingleton(paymentsRepository)))
    //         .CreateClient();
    //
    //     // Act
    //     var response = await client.GetAsync($"/api/Payments/{payment.PaymentId}");
    //     var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentDto>();
    //     
    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     Assert.NotNull(paymentResponse);
    // }
    //
    // [Fact(Skip = "skip for now")]
    // public async Task Returns404IfPaymentNotFound()
    // {
    //     // Arrange
    //     var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
    //     var client = webApplicationFactory.CreateClient();
    //     
    //     // Act
    //     var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
    //     
    //     // Assert
    //     Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    // }
}