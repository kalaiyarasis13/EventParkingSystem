using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IServices;

public interface IHoldService
{
    Task<HoldResponse> CreateAsync(
        int customerId,
        CreateHoldRequest request);

    Task<HoldResponse> GetByIdAsync(int holdId);

    Task<List<HoldResponse>> GetMyActiveHoldsAsync(
        int customerId);

    Task ReleaseAsync(
        int holdId,
        int customerId);

    Task ExpireAsync(int holdId);
}