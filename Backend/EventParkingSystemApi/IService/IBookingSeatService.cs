using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IService;

public interface IBookingSeatService
{
    Task<List<BookingSeatResponse>> GetByBookingIdAsync(
        int bookingId);

    Task<BookingSeatResponse> GetByIdAsync(
        int bookingSeatId);

    Task DeleteAsync(
        int bookingSeatId);
}