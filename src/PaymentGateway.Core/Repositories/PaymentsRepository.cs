using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using PaymentGateway.Core.Models;

namespace PaymentGateway.Core.Repositories;

// Excluding this from code coverage because it's not a real implementation anyway
[ExcludeFromCodeCoverage]
public class PaymentsRepository : IPaymentsRepository
{
    // switching this to a concurrent dictionary just in case unit test parallelization runs amok
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    public void Add(Payment payment)
    {
        _payments[payment.PaymentId] = payment;
    }

    public Payment? GetById(Guid id)
    {
        return _payments.GetValueOrDefault(id);
    }
}