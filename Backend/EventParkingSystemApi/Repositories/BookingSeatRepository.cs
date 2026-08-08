using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Repositories;

public class BookingSeatRepository : IBookingSeatRepository
{
    private readonly AppDbContext _db;

    public BookingSeatRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BookingSeat?> GetByIdAsync(int bookingSeatId)
    {
        return await _db.BookingSeats
            .Include(bs => bs.Booking)
            .Include(bs => bs.Seat)
            .FirstOrDefaultAsync(
                bs => bs.BookingSeatId == bookingSeatId);
    }

    public async Task<List<BookingSeat>> GetByBookingIdAsync(
        int bookingId)
    {
        return await _db.BookingSeats
            .Include(bs => bs.Seat)
            .Where(bs => bs.BookingId == bookingId)
            .OrderBy(bs => bs.Seat!.SeatRow)
            .ThenBy(bs => bs.Seat!.SeatNumber)
            .ToListAsync();
    }

    public async Task<BookingSeat?> GetBySeatIdAsync(
        int seatId)
    {
        return await _db.BookingSeats
            .Include(bs => bs.Booking)
            .Include(bs => bs.Seat)
            .FirstOrDefaultAsync(
                bs => bs.SeatId == seatId);
    }

    public async Task AddAsync(BookingSeat bookingSeat)
    {
        await _db.BookingSeats.AddAsync(bookingSeat);
    }

    public void Update(BookingSeat bookingSeat)
    {
        _db.BookingSeats.Update(bookingSeat);
    }

    public void Remove(BookingSeat bookingSeat)
    {
        _db.BookingSeats.Remove(bookingSeat);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}