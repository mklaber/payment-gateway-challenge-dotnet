using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Contracts;

/// <summary>
/// Represents a payment that has already been attempted
/// </summary>
/// <remarks>Data Annotations are, again, used to help create more useful swagger specs</remarks>
public class PaymentDto
{
    /// <summary>
    /// Unique identifier of this payment attempt
    /// </summary>
    [Required]
    public Guid PaymentId { get; set; }
    [Required]
    public PaymentStatusDto Status { get; set; }
    [Required]
    public int CardNumberLastFour { get; set; }
    [Required, Range(1, 12)]
    public int ExpiryMonth { get; set; }
    [Required]
    public int ExpiryYear { get; set; }
    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = null!;
    [Required]
    public int Amount { get; set; }
}
