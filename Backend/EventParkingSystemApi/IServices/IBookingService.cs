using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IServices;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest request);

    Task<List<BookingResponse>> GetByCustomerAsync(int customerId);

    Task<List<BookingResponse>> GetByEventAsync(int eventId);

    Task<BookingResponse> GetByIdAsync(int id);

    Task CancelAsync(
        int id,
        int requestingCustomerId,
        bool isAdmin);
}