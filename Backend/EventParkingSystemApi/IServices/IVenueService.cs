using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IServices;

public interface IVenueService
{
    Task<VenueResponse> CreateAsync(
        CreateVenueRequest request);

    Task<VenueResponse> GetByIdAsync(
        int venueId);

    Task<List<VenueResponse>> GetAllAsync();

    Task<VenueResponse> UpdateAsync(
        int venueId,
        UpdateVenueRequest request);

    Task DeleteAsync(
        int venueId);
}