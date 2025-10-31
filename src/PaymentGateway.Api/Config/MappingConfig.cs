using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Mapster;

namespace PaymentGateway.Api.Config;

[ExcludeFromCodeCoverage]
public static class MappingConfig
{
    public static void RegisterMappings(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddMapster();
    }
}