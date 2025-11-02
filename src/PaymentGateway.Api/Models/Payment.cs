namespace PaymentGateway.Api.Models;

/// <summary>
/// A "Payment" is an attempt at creating a payment; it may or may not have been successful.
/// </summary>
public class Payment
{
    public Guid PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? BankAuthorizationCode { get; set; }
    public int CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; } = null!;
    public int Amount { get; set; }
}