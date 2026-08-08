using EventParkingSystemApi.Models;


namespace EventParkingSystemApi.IRepositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task AddAsync(Payment payment);
    void Update(Payment payment);
    void Remove(Payment payment);
    Task<int> SaveChangesAsync();
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();

    Task<List<Payment>> GetByCustomerWithBookingAsync(int customerId);
    Task<Payment?> GetByIdWithReceiptDetailsAsync(int paymentId);
    Task<decimal> SumTotalRevenueAsync();
}
