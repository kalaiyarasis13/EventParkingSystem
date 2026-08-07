using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.Data;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventParkingSystemApi.IRepositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    public BookingRepository(AppDbContext db) => _db = db;

    public async Task<Booking?> GetByIdAsync(int id) => await _db.Bookings.FindAsync(id);

    public async Task AddAsync(Booking booking) => await _db.Bookings.AddAsync(booking);

    public void Update(Booking booking) => _db.Bookings.Update(booking);

    public void Remove(Booking booking) => _db.Bookings.Remove(booking);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync() => await _db.Database.BeginTransactionAsync();

    private IQueryable<Booking> WithFullDetails() =>
        _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Event)
            .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
            .Include(b => b.ParkingReservation).ThenInclude(pr => pr!.ParkingSlot)
            .Include(b => b.Payment);

    public async Task<Booking?> GetByIdWithFullDetailsAsync(int bookingId) =>
        await WithFullDetails().FirstOrDefaultAsync(b => b.BookingId == bookingId);

    public async Task<List<Booking>> GetByCustomerWithDetailsAsync(int customerId) =>
        await WithFullDetails().Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt).ToListAsync();

    public async Task<List<Booking>> GetByEventWithDetailsAsync(int eventId) =>
        await WithFullDetails().Where(b => b.EventId == eventId)
            .OrderByDescending(b => b.CreatedAt).ToListAsync();

    // Business Rule: an event can only be deleted if it has no active (non-cancelled) bookings.
    public async Task<bool> HasActiveBookingsForEventAsync(int eventId) =>
        await _db.Bookings.AnyAsync(b => b.EventId == eventId && b.Status != BookingStatus.Cancelled);

    public async Task<int> CountActiveByCustomerAsync(int customerId) =>
        await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Status != BookingStatus.Cancelled);

    public async Task<int> CountUpcomingByCustomerAsync(int customerId) =>
        await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Status != BookingStatus.Cancelled
            && b.Event!.EventDate >= DateOnly.FromDateTime(DateTime.UtcNow));

    public async Task<int> CountActiveAsync() =>
        await _db.Bookings.CountAsync(b => b.Status != BookingStatus.Cancelled);
}
