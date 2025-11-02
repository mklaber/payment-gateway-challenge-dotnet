using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Contracts;

/// <summary>
/// Represents a payment that has already been attempted
/// </summary>
/// <remarks>Data Annotations are, again, used to help create more useful swagger specs</remarks>
public class PaymentDto
{
    /// <summary>
    /// Unique identifier of this Payment
    /// </summary>
    [Required]
    public Guid PaymentId { get; set; }

    /// <summary>
    /// The status of this Payment: `Authorized` or `Declined`
    /// </summary>
    [Required]
    public PaymentStatusDto Status { get; set; }

    /// <summary>
    /// The last 4 digits of the Card Number used for this Payment
    /// </summary>
    [Required]
    public int CardNumberLastFour { get; set; }

    /// <summary>
    /// The month this payment's method expires
    /// </summary>
    [Required, Range(1, 12)]
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// The year this payment's method expires
    /// </summary>
    [Required]
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Three character ISO currency code as specified by ISO-4217
    /// </summary>
    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = null!;

    /// <summary>
    /// The payment amount in minor currency units
    /// </summary>
    [Required]
    public int Amount { get; set; }
}