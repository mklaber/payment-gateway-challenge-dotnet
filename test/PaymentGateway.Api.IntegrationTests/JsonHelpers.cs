using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.IntegrationTests;

public static class JsonHelpers
{
    public static JsonSerializerOptions StandardOptions => new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}