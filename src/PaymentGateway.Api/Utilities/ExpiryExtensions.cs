using PaymentGateway.Api.Contracts;

namespace PaymentGateway.Api.Utilities;

public static class ExpiryExtensions
{
    private static DateOnly ToExpiryDate(int year, int month) => new DateOnly(year, month, 1);

    private static bool TryToExpiryDate(int? year, int? month, out DateOnly expiryDate)
    {
        if (year is null || month is null)
        {
            expiryDate = DateOnly.MinValue;
            return false;
        }
        try
        {
            expiryDate = ToExpiryDate(year!.Value, month!.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException e)
        {
            expiryDate = DateOnly.MinValue;
            return false;
        }
    }

    public static bool TryToExpiryDate(this CreatePaymentRequest createPaymentRequest, out DateOnly expiryDate) =>
        TryToExpiryDate(createPaymentRequest.ExpiryYear, createPaymentRequest.ExpiryMonth, out expiryDate);

    public static DateOnly ToExpiryDate(this CreatePaymentRequest request) =>
        ToExpiryDate(request.ExpiryYear!.Value, request.ExpiryMonth!.Value);

    public static DateOnly ToExpiryDate(this DateTime date) => ToExpiryDate(date.Year, date.Month);
    
}