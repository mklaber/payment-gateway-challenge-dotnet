using System.Text.Json;

using FluentResults;

using PaymentGateway.Api.Results;

namespace PaymentGateway.Api.Clients.Mountebank;

public class MountebankClient(HttpClient httpClient, ILogger<MountebankClient> logger) : IMountebankClient
{
    private static readonly JsonSerializerOptions MountebankJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, WriteIndented = true
    };

    public async Task<Result<PaymentResponseExternalDto>> CreatePaymentAsync(
        PaymentRequestExternalDto paymentRequestDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var resp = await httpClient.PostAsJsonAsync("payments", paymentRequestDto, MountebankJsonOptions,
                cancellationToken);
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<PaymentResponseExternalDto>(MountebankJsonOptions,
                cancellationToken))!;
        }
        catch (HttpRequestException e) when (e.StatusCode is not null)
        {
            var statusCodeInt = (int)e.StatusCode;
            var isPermanent = statusCodeInt is >= 400 and < 500 && statusCodeInt != 429;
            logger.LogWarning(e,
                "Failed to create payment in the amount of {Amount} and will report it as IsTransient: {IsPermanent}",
                paymentRequestDto.Amount, !isPermanent);
            return Result.Fail(new BankError(!isPermanent).CausedBy(e));
        }
        // Any other exception is truly exceptional; we do not want to swallow it (it doesn't matter that it's from the bank)
    }
}