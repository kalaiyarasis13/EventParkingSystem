using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventParkingSystemApi.IRepository;

public interface IBookingRepository
{
    // Basic CRUD
    Task<Booking?> GetByIdAsync(int id);

    Task AddAsync(Booking booking);

    void Update(Booking booking);

    void Remove(Booking booking);

    Task<int> SaveChangesAsync();

    // Transaction
    Task<IDbContextTransaction> BeginTransactionAsync();

    // Booking details
    Task<Booking?> GetByIdWithFullDetailsAsync(int bookingId);

    Task<List<Booking>> GetByCustomerWithDetailsAsync(int customerId);

    Task<List<Booking>> GetByEventWithDetailsAsync(int eventId);

    // Business rules
    Task<bool> HasActiveBookingsForEventAsync(int eventId);

    Task<int> CountActiveByCustomerAsync(int customerId);

    Task<int> CountUpcomingByCustomerAsync(int customerId);

    Task<int> CountActiveAsync();
}