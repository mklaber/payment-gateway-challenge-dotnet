using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.IntegrationTests.UseCases;

public class GetPaymentTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Gets_Payment_Successfully()
    {
        // Arrange
        PaymentsRepository paymentRepo = new(() => TestAuthHandler.GetTestMerchant());
        Payment payment = new()
        {
            PaymentId = Guid.NewGuid(),
            Status = PaymentStatus.Declined,
            CardNumberLastFour = 4321,
            ExpiryYear = DateTime.Now.Year + 1,
            ExpiryMonth = DateTime.Now.Month,
            Currency = "EUR",
            Amount = 91231,
            BankAuthorizationCode = "some-auth"
        };
        paymentRepo.Add(payment);

        HttpClient client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton<IPaymentsRepository>(paymentRepo);
            });
            builder.ConfigureTestServices(TestAuthHandler.ConfigureTestServices);
        }).CreateClient();
        TestAuthHandler.ConfigureHttpClient(client);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/payments/{payment.PaymentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PaymentDto? retrievedPayment =
            await response.Content.ReadFromJsonAsync<PaymentDto>(JsonHelpers.StandardOptions);
        retrievedPayment.Should().NotBeNull();

        retrievedPayment.Should().BeEquivalentTo(payment, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Gets_404_NotFound_For_Payment_Owned_By_Different_Merchant()
    {
        // Arrange
        var merchantToReturn = TestAuthHandler.GetTestMerchant("initial-merchant");
        PaymentsRepository paymentRepo = new(() => merchantToReturn);
        Payment payment = new()
        {
            PaymentId = Guid.NewGuid(),
            Status = PaymentStatus.Declined,
            CardNumberLastFour = 4321,
            ExpiryYear = DateTime.Now.Year + 1,
            ExpiryMonth = DateTime.Now.Month,
            Currency = "EUR",
            Amount = 91231,
            BankAuthorizationCode = "some-auth"
        };
        paymentRepo.Add(payment);

        HttpClient client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton<IPaymentsRepository>(paymentRepo);
            });
            builder.ConfigureTestServices(services =>
            {
                TestAuthHandler.ConfigureTestServices(services);
            });
        }).CreateClient();
        var differentMerchantId = "different-merchant";
        merchantToReturn.ChangeMerchantId(differentMerchantId);
        TestAuthHandler.ConfigureHttpClient(client, differentMerchantId);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/payments/{payment.PaymentId}");

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.NotFound, "the payment ID exists but is for a different merchant");

        ProblemDetails? problemDetails =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonHelpers.StandardOptions);
        problemDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task Gets_404_NotFound_For_Payment_That_Does_Not_Exist()
    {
        // Arrange
        HttpClient client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(TestAuthHandler.ConfigureTestServices);
        }).CreateClient();
        TestAuthHandler.ConfigureHttpClient(client);
        // Act
        HttpResponseMessage response = await client.GetAsync($"/payments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        ProblemDetails? problemDetails =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonHelpers.StandardOptions);
        problemDetails.Should().NotBeNull();
    }
}