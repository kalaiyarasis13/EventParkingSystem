using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IService;

public interface ISeatService
{
    Task<SeatResponse> CreateAsync(
        CreateSeatRequest request);

    Task<SeatResponse> GetByIdAsync(
        int seatId);

    Task<List<SeatResponse>> GetByEventAsync(
        int eventId);

    Task<List<SeatResponse>> GetAvailableByEventAsync(
        int eventId);

    Task<SeatResponse> UpdateAsync(
        int seatId,
        UpdateSeatRequest request);

    Task DeleteAsync(
        int seatId);
}