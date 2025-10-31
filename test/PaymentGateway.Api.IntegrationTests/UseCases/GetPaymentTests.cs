using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Repositories;

namespace PaymentGateway.Api.IntegrationTests.UseCases;

public class GetPaymentTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Gets_Payment_Successfully()
    {
        // Arrange
        PaymentsRepository paymentRepo = new();
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
        }).CreateClient();

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
    public async Task Gets_404_NotFound()
    {
        // Arrange

        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/payments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        ProblemDetails? problemDetails =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonHelpers.StandardOptions);
        problemDetails.Should().NotBeNull();
    }
}