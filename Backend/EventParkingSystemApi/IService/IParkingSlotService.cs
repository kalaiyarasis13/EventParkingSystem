using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IService;

public interface IParkingSlotService
{
    Task<ParkingSlotResponse> CreateAsync(
        CreateParkingSlotRequest request);

    Task<ParkingSlotResponse> GetByIdAsync(
        int parkingSlotId);

    Task<List<ParkingSlotResponse>> GetByEventAsync(
        int eventId);

    Task<List<ParkingSlotResponse>> GetAvailableByEventAsync(
        int eventId);

    Task<ParkingSlotResponse> UpdateAsync(
        int parkingSlotId,
        UpdateParkingSlotRequest request);

    Task DeleteAsync(
        int parkingSlotId);
}