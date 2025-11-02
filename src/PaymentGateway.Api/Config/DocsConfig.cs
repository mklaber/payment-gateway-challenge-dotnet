using System.Diagnostics.CodeAnalysis;

using NSwag;
using NSwag.Generation.Processors.Security;

namespace PaymentGateway.Api.Config;

[ExcludeFromCodeCoverage]
public static class DocsConfig
{
    public static void AddApiDocServices(this IServiceCollection services)
    {
        services.AddOpenApiDocument(options =>
        {
            options.PostProcess = doc =>
            {
                doc.Info.Version = "v1";
                doc.Info.Title = "Payment Gateway API";
                doc.Info.Description = """
                                       Payments are a way to allow a Merchant to offer a way for their shoppers to pay for their products.

                                       Endpoints that require authentication expect a JWT token with the `Bearer` scheme 
                                       in the `Authorization` header. These tokens are unique per Merchant and are generated 
                                       using magical pixie dust.

                                       No one likes it when an API returns an error. We do our best to describe what's happened.
                                       Generally speaking, errors in the `4xx` range should not be retried without a change
                                       to the request (e.g., fix a validation error, reset the JWT token, etc.). Errors in
                                       the `5xx` range should treated as transient; retrying after a short while (use of an
                                       exponential backoff strategy is recommended) should resolve the issue.

                                       Unfortunately, because we rely on some external systems, we can't be certain a retry
                                       won't have unintended consequences such as double payments.
                                       """;
            };

            options.AddSecurity("jwt-bearer",
                new OpenApiSecurityScheme()
                {
                    Type = OpenApiSecuritySchemeType.Http,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Scheme = "Bearer",
                    Description = "Merchant-specific JWT token using the Bearer scheme"
                });

            options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("jwt-bearer"));
        });
    }

    public static void UseApiDocs(this IApplicationBuilder app)
    {
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.UseReDoc(opts =>
        {
            opts.Path = "/redoc";
        });
    }
}