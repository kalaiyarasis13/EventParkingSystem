using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepository;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(int seatId);

    Task<List<Seat>> GetByEventAsync(int eventId);

    Task<List<Seat>> GetAvailableByEventAsync(int eventId);

    Task<List<Seat>> GetByIdsForEventAsync(
        List<int> seatIds,
        int eventId);

    Task AddAsync(Seat seat);

    void Update(Seat seat);

    void Remove(Seat seat);

    Task<int> SaveChangesAsync();
}