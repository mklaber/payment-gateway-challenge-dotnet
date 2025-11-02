using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PaymentGateway.Api.Identity;

namespace PaymentGateway.Api.IntegrationTests;

/// <summary>
/// This is a test fixture that configures authentication requirements during integration tests without needing
/// to generate test JWTs.
/// </summary>
/// <remarks>Ideally we'd just use the same `dotnet user-jwts` as the development environment, but
/// it's not available in code and calling a command line app in a fixture gets even more complicated
/// than this current arrangement.</remarks>
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string DefaultMerchantId = "test-merchant-abc";
    private const string SchemeName = "TestScheme";
    private const string MerchantIdOverrideHeaderKey = "x-merchant-id";

    /// <summary>
    /// Configure the factory app's Test Services to use our test auth apparatus
    /// </summary>
    public static void ConfigureTestServices(IServiceCollection services)
    {
        services.AddAuthentication(defaultScheme: SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                SchemeName, options => { });
    }

    /// <summary>
    /// Configure <paramref name="client"/> to authenticate using our test auth apparatus, optionally
    /// overriding the merchant ID we're claiming to be.
    /// </summary>
    public static void ConfigureHttpClient(HttpClient client, string? merchantIdOverride = null)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(SchemeName);
        if (merchantIdOverride != null)
        {
            client.DefaultRequestHeaders.Add(MerchantIdOverrideHeaderKey, merchantIdOverride);
        }
    }

    /// <summary>
    /// Gets a test implementation of <see cref="IMerchant"/> that allows us to change the Merchant ID
    /// </summary>
    /// <remarks>If our repository was scoped rather than singleton, we wouldn't need any of this malarchy.</remarks>
    public static TestMerchant GetTestMerchant(string? merchantIdOverride = null) =>
        new(merchantIdOverride ?? DefaultMerchantId);

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var merchantId = DefaultMerchantId;
        if (Request.Headers.TryGetValue(MerchantIdOverrideHeaderKey, out var merchantIdSv))
        {
            merchantId = merchantIdSv.FirstOrDefault() ?? DefaultMerchantId;
        }

        var claims = new[] { new Claim(ClaimTypes.Name, merchantId) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }

    /// <summary>
    /// An implementation of <see cref="IMerchant"/> that allows us to explicitly set, and then change, the Merchant ID
    /// </summary>
    public sealed class TestMerchant(string merchantId) : IMerchant
    {
        private string _merchantId = merchantId;
        public string MerchantId => _merchantId;

        public void ChangeMerchantId(string newMerchantId)
        {
            _merchantId = newMerchantId;
        }
    }
}