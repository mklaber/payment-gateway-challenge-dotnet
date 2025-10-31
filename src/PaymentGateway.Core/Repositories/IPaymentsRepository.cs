using PaymentGateway.Core.Models;

namespace PaymentGateway.Core.Repositories;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment? GetById(Guid id);
}