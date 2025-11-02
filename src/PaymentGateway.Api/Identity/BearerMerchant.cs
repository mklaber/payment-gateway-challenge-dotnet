using PaymentGateway.Core.Identity;

namespace PaymentGateway.Api.Identity;

public class BearerMerchant(IHttpContextAccessor ctx) : IMerchant
{
    // meh, we should not support anonymous but we'll just use it to deal with nulls for now
    public string MerchantId => ctx.HttpContext?.User?.Identity?.Name ?? "anonymous";
}