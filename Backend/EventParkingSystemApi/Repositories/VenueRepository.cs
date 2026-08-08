using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _db;

    public VenueRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Venue?> GetByIdAsync(int venueId)
    {
        return await _db.Venues
            .Include(v => v.Events)
            .FirstOrDefaultAsync(v => v.VenueId == venueId);
    }

    public async Task<List<Venue>> GetAllAsync()
    {
        return await _db.Venues
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Venue?> GetByNameAsync(string name)
    {
        return await _db.Venues
            .FirstOrDefaultAsync(v => v.Name == name);
    }

    public async Task AddAsync(Venue venue)
    {
        await _db.Venues.AddAsync(venue);
    }

    public void Update(Venue venue)
    {
        _db.Venues.Update(venue);
    }

    public void Remove(Venue venue)
    {
        _db.Venues.Remove(venue);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}