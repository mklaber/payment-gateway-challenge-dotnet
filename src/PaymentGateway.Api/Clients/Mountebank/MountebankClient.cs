using System.Text.Json;

namespace PaymentGateway.Api.Clients.Mountebank;

public class MountebankClient(HttpClient httpClient) : IMountebankClient
{
    private static readonly JsonSerializerOptions MountebankJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, WriteIndented = true
    };

    public async Task<PaymentResponseExternalDto> CreatePaymentAsync(PaymentRequestExternalDto paymentRequestDto,
        CancellationToken cancellationToken)
    {
        var resp = await httpClient.PostAsJsonAsync("payments", paymentRequestDto, MountebankJsonOptions,
            cancellationToken);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<PaymentResponseExternalDto>(MountebankJsonOptions,
            cancellationToken))!;
    }
}