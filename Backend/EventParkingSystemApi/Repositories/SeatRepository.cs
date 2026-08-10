using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _db;

    public SeatRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Seat?> GetByIdAsync(int seatId)
    {
        return await _db.Seats
            .Include(s => s.Event)
            .Include(s => s.BookingSeat)
            .FirstOrDefaultAsync(s => s.SeatId == seatId);
    }

    public async Task<List<Seat>> GetByEventAsync(int eventId)
    {
        return await _db.Seats
            .Where(s => s.EventId == eventId)
            .OrderBy(s => s.SeatRow)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();
    }

    public async Task<List<Seat>> GetAvailableByEventAsync(int eventId)
    {
        return await _db.Seats
            .Where(s =>
                s.EventId == eventId &&
                s.Status == SeatStatus.Available)
            .OrderBy(s => s.SeatRow)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();
    }

    public async Task<List<Seat>> GetByIdsForEventAsync(
        List<int> seatIds,
        int eventId)
    {
        return await _db.Seats
            .Where(s =>
                seatIds.Contains(s.SeatId) &&
                s.EventId == eventId)
            .OrderBy(s => s.SeatRow)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();
    }

    public async Task AddAsync(Seat seat)
    {
        await _db.Seats.AddAsync(seat);
    }

    public void Update(Seat seat)
    {
        _db.Seats.Update(seat);
    }

    public void Remove(Seat seat)
    {
        _db.Seats.Remove(seat);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }

    public async Task<int> CountAvailableAsync()
    {
        return await _db.Seats
            .CountAsync(s => s.Status == SeatStatus.Available);
    }
}