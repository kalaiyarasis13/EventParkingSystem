using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepository;

public interface IHoldRepository
{
    Task<SeatHold?> GetByIdAsync(int holdId);

    Task<SeatHold?> GetActiveHoldBySeatAsync(int seatId);

    Task<List<SeatHold>> GetActiveHoldsByCustomerAsync(int customerId);

    Task AddAsync(SeatHold hold);

    void Update(SeatHold hold);

    void Remove(SeatHold hold);

    Task<int> SaveChangesAsync();
}