using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepositories;

public interface IParkingSlotRepository
{
    Task<ParkingSlot?> GetByIdAsync(int parkingSlotId);

    Task<ParkingSlot?> GetByIdForEventAsync(
        int parkingSlotId,
        int eventId);

    Task<List<ParkingSlot>> GetByEventAsync(
        int eventId);

    Task<List<ParkingSlot>> GetAvailableByEventAsync(
        int eventId);

    Task<int> CountOccupiedAsync();

    Task AddAsync(ParkingSlot parkingSlot);

    void Update(ParkingSlot parkingSlot);

    void Remove(ParkingSlot parkingSlot);

    Task<int> SaveChangesAsync();
}