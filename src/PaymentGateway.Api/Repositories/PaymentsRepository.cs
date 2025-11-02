using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using PaymentGateway.Api.Identity;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Repositories;

// Excluding this from code coverage because it's not a real implementation anyway
/// <summary>
/// An in-memory implementation of <see cref="IPaymentsRepository"/>
/// </summary>
/// <param name="currentMerchant">A factory that returns the current <see cref="IMerchant"/> which may be at a different scope than the repository</param>
/// <remarks>The <paramref name="currentMerchant"/> only exists because we can't scope the repo to the request and
/// still rely on its singleton, in-memory characteristics.
///
/// Because this malarkey is so unusual, we're also excluding it from test coverage requirements.
/// </remarks>
[ExcludeFromCodeCoverage]
public class PaymentsRepository(Func<IMerchant> currentMerchant) : IPaymentsRepository
{
    // using a concurrent dictionary just in case unit test parallelization runs amok
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Payment>> _payments = new();

    public void Add(Payment payment)
    {
        var merchId = currentMerchant().MerchantId;
        _payments.TryAdd(merchId, new ConcurrentDictionary<Guid, Payment>());
        _payments[merchId][payment.PaymentId] = payment;
    }

    public Payment? GetById(Guid id)
    {
        var merchId = currentMerchant().MerchantId;
        return _payments.GetValueOrDefault(merchId)?.GetValueOrDefault(id);
    }
}