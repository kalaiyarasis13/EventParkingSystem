using EventParkingSystemApi.Data;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventParkingSystemApi.IRepositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;
    public PaymentRepository(AppDbContext db) => _db = db;

    public async Task<Payment?> GetByIdAsync(int id) => await _db.Payments.FindAsync(id);

    public async Task AddAsync(Payment payment) => await _db.Payments.AddAsync(payment);

    public void Update(Payment payment) => _db.Payments.Update(payment);

    public void Remove(Payment payment) => _db.Payments.Remove(payment);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync() => await _db.Database.BeginTransactionAsync();

    public async Task<List<Payment>> GetByCustomerWithBookingAsync(int customerId) =>
        await _db.Payments.Include(p => p.Booking)
            .Where(p => p.Booking!.CustomerId == customerId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

    public async Task<Payment?> GetByIdWithReceiptDetailsAsync(int paymentId) =>
        await _db.Payments
            .Include(p => p.Booking).ThenInclude(b => b!.Customer)
            .Include(p => p.Booking).ThenInclude(b => b!.Event)
            .Include(p => p.Booking).ThenInclude(b => b!.BookingSeats).ThenInclude(bs => bs.Seat)
            .Include(p => p.Booking).ThenInclude(b => b!.ParkingReservation).ThenInclude(pr => pr!.ParkingSlot)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

    public async Task<decimal> SumTotalRevenueAsync() =>
        await _db.Payments.SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;
}
