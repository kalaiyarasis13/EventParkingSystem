using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepositories;

public interface IParkingReservationRepository
{
    Task<ParkingReservation?> GetByIdAsync(int parkingReservationId);

    Task<ParkingReservation?> GetByBookingIdAsync(int bookingId);

    Task<ParkingReservation?> GetByParkingSlotIdAsync(int parkingSlotId);

    Task<List<ParkingReservation>> GetByCustomerAsync(int customerId);

    Task AddAsync(ParkingReservation reservation);

    void Update(ParkingReservation reservation);

    void Remove(ParkingReservation reservation);

    Task<int> SaveChangesAsync();
}