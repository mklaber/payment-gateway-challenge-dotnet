using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Repositories;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment? GetById(Guid id);
}