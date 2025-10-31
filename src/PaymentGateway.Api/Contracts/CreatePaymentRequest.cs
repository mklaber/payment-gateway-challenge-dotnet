using System.ComponentModel.DataAnnotations;

using MediatR;

namespace PaymentGateway.Api.Contracts;

/// <summary>
/// Service result response that wraps validation errors and successes.
/// </summary>
/// <remarks>Given more time, we'd catch validation exceptions and add a .NET filter</remarks>
public record SuccessOrFailure<T>(T? Success, IDictionary<string, string[]>? Failures = null);

public class CreatePaymentRequest : IRequest<SuccessOrFailure<PaymentDto>>
{
    /// <summary>
    /// Credit Card number containing 14 to 19 numeric characters only
    /// </summary>
    [Required, StringLength(19, MinimumLength = 14)]
    public string? CardNumber { get; set; }
    /// <summary>
    /// Expiration month, from 1 to 12. Combined with <see cref="ExpiryYear"/>, this must be a date in the future.
    /// </summary>
    [Required, Range(1, 12)]
    public int? ExpiryMonth { get; set; }
    /// <summary>
    /// Expiration year, four digits. Combined with <see cref="ExpiryMonth"/>, thus must be a date in the future.
    /// </summary>
    [Required]
    public int? ExpiryYear { get; set; }
    /// <summary>
    /// Three character ISO currency code as specified by ISO-4217 for a country supported by this service.
    /// </summary>
    [Required, StringLength(3,  MinimumLength = 3)]
    public string? Currency { get; set; }
    /// <summary>
    /// The payment amount in minor currency units.
    /// </summary>
    [Required]
    public int? Amount { get; set; }
    /// <summary>
    /// Card security code containing 3 to 4 numeric characters only
    /// </summary>
    [Required, StringLength(4, MinimumLength = 3)]
    public string? Cvv { get; set; }
}