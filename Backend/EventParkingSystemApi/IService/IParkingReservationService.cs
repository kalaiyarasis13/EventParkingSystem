using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IService;

public interface IParkingReservationService
{
    Task<ParkingReservationResponse> CreateAsync(
        int customerId,
        CreateParkingReservationRequest request);

    Task<ParkingReservationResponse> GetByIdAsync(
        int parkingReservationId);

    Task<ParkingReservationResponse?> GetByBookingIdAsync(
        int bookingId);

    Task<List<ParkingReservationResponse>> GetMyReservationsAsync(
        int customerId);

    Task CancelAsync(
        int parkingReservationId,
        int customerId);
}