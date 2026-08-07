using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.IRepositories;

public class HoldRepository : IHoldRepository
{
    private readonly AppDbContext _db;

    public HoldRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SeatHold?> GetByIdAsync(int holdId)
    {
        return await _db.SeatHolds
            .Include(h => h.Seat)
            .Include(h => h.Event)
            .Include(h => h.Customer)
            .FirstOrDefaultAsync(h => h.HoldId == holdId);
    }

    public async Task<SeatHold?> GetActiveHoldBySeatAsync(int seatId)
    {
        return await _db.SeatHolds
            .FirstOrDefaultAsync(h =>
                h.SeatId == seatId &&
                h.Status == SeatHoldStatus.Active &&
                h.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<List<SeatHold>> GetActiveHoldsByCustomerAsync(
        int customerId)
    {
        return await _db.SeatHolds
            .Where(h =>
                h.CustomerId == customerId &&
                h.Status == SeatHoldStatus.Active &&
                h.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(h => h.HeldAt)
            .ToListAsync();
    }

    public async Task AddAsync(SeatHold hold)
    {
        await _db.SeatHolds.AddAsync(hold);
    }

    public void Update(SeatHold hold)
    {
        _db.SeatHolds.Update(hold);
    }

    public void Remove(SeatHold hold)
    {
        _db.SeatHolds.Remove(hold);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}