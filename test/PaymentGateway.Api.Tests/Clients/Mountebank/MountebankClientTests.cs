using System.Net;

using PaymentGateway.Api.Clients.Mountebank;

namespace PaymentGateway.Api.Tests.Clients.Mountebank;

public class MountebankClientTests
{

    [Theory]
    [InlineData(false, "", "even if it is not authorized it is still a successful call")]
    [InlineData(true, "any-old-auth-code-1234", "authorized calls are the golden path")]
    public async Task CreatePaymentAsync_WhenSuccessful_ReturnsPaymentResponse(bool isAuthorized, string code,
        string because)
    {
        // Arrange
        var request = new PaymentRequestExternalDto
        {
            Amount = 100,
            Currency = "USD",
            CardNumber = "4111111111111111",
            Cvv = "123",
            ExpiryDate = "12/2030"
        };

        var expectedRequestJson = $$"""
                                    {
                                      "card_number": "{{request.CardNumber}}",
                                      "expiry_date": "{{request.ExpiryDate}}",
                                      "currency": "{{request.Currency}}",
                                      "amount": {{request.Amount}},
                                      "cvv": "{{request.Cvv}}"
                                    }
                                    """;

        var expectedResponseJson = $$"""
                                     {
                                       "authorized": {{isAuthorized.ToString().ToLower()}},
                                       "authorization_code": "{{code}}"
                                     }
                                     """;

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK, Content = new StringContent(expectedResponseJson)
        };

        var fakeHttpMessageHandler = new FakeHttpMessageHandler(response);

        var sut = new MountebankClient(new HttpClient(fakeHttpMessageHandler)
        {
            BaseAddress = new Uri("http://localhost:5000")
        });

        // Act
        var result = await sut.CreatePaymentAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull(because);
        result.AuthorizationCode.Should().Be(code);
        result.Authorized.Should().Be(isAuthorized);


        var actualRequest = fakeHttpMessageHandler.Request!;
        actualRequest.RequestUri!.AbsolutePath.Should().Be("/payments");
        actualRequest.Method.Should().Be(HttpMethod.Post);

        var actualRequestBody = await actualRequest.Content!.ReadAsStringAsync();
        actualRequestBody.Should()
            .BeEquivalentTo(expectedRequestJson, o => o.IgnoringLeadingWhitespace().IgnoringNewlineStyle());

    }

    [Fact]
    public async Task CreatePaymentAsync_WhenHttpError_ThrowsException()
    {
        // Arrange
        var request = new PaymentRequestExternalDto { Amount = 100, Currency = "USD", CardNumber = "4111111111111111" };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.ServiceUnavailable,
            Content = new StringContent("does not matter, will not read")
        };

        var fakeHttpMessageHandler = new FakeHttpMessageHandler(response);

        var sut = new MountebankClient(new HttpClient(fakeHttpMessageHandler)
        {
            BaseAddress = new Uri("http://localhost:5000")
        });


        // Act
        var act = async () => await sut.CreatePaymentAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();

    }

    /// <summary>
    /// For super easy HTTP Client testing
    /// </summary>
    private class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(response);
        }
    }
}