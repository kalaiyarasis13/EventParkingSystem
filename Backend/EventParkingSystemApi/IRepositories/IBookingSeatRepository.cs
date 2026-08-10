using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepositories;

public interface IBookingSeatRepository
{
    Task<BookingSeat?> GetByIdAsync(int bookingSeatId);

    Task<List<BookingSeat>> GetByBookingIdAsync(int bookingId);

    Task<BookingSeat?> GetBySeatIdAsync(int seatId);

    Task AddAsync(BookingSeat bookingSeat);

    void Update(BookingSeat bookingSeat);

    void Remove(BookingSeat bookingSeat);

    Task<int> SaveChangesAsync();
}