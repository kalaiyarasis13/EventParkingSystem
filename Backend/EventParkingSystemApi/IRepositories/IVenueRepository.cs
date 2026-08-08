using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepositories;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(int venueId);

    Task<List<Venue>> GetAllAsync();

    Task<Venue?> GetByNameAsync(string name);

    Task AddAsync(Venue venue);

    void Update(Venue venue);

    void Remove(Venue venue);

    Task<int> SaveChangesAsync();

    Task<bool> ExistsAsync(int venueId);
}