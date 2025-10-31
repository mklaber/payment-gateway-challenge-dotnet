namespace PaymentGateway.Api.Clients.Mountebank;

public record PaymentRequestExternalDto()
{
    public string? CardNumber { get; set; }
    public string? ExpiryDate { get; set; }
    public string? Currency { get; set; }
    public int Amount { get; set; }
    public string? Cvv { get; set; }
}