using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Repositories;

public class ParkingSlotRepository : IParkingSlotRepository
{
    private readonly AppDbContext _db;

    public ParkingSlotRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ParkingSlot?> GetByIdAsync(int parkingSlotId)
    {
        return await _db.ParkingSlots
            .Include(p => p.Event)
            .Include(p => p.ParkingReservation)
            .FirstOrDefaultAsync(
                p => p.ParkingSlotId == parkingSlotId);
    }

    public async Task<ParkingSlot?> GetByIdForEventAsync(
        int parkingSlotId,
        int eventId)
    {
        return await _db.ParkingSlots
            .Include(p => p.Event)
            .Include(p => p.ParkingReservation)
            .FirstOrDefaultAsync(
                p => p.ParkingSlotId == parkingSlotId &&
                     p.EventId == eventId);
    }

    public async Task<List<ParkingSlot>> GetByEventAsync(
        int eventId)
    {
        return await _db.ParkingSlots
            .Where(p => p.EventId == eventId)
            .OrderBy(p => p.SlotLabel)
            .ToListAsync();
    }

    public async Task<List<ParkingSlot>> GetAvailableByEventAsync(
        int eventId)
    {
        return await _db.ParkingSlots
            .Where(p =>
                p.EventId == eventId &&
                p.Status == ParkingSlotStatus.Available)
            .OrderBy(p => p.SlotLabel)
            .ToListAsync();
    }

    public async Task AddAsync(ParkingSlot parkingSlot)
    {
        await _db.ParkingSlots.AddAsync(parkingSlot);
    }

    public void Update(ParkingSlot parkingSlot)
    {
        _db.ParkingSlots.Update(parkingSlot);
    }

    public void Remove(ParkingSlot parkingSlot)
    {
        _db.ParkingSlots.Remove(parkingSlot);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}