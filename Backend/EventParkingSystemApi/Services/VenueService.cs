using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.IService;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venueRepository;

    public VenueService(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }

    public async Task<VenueResponse> CreateAsync(
        CreateVenueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw ApiException.BadRequest("Venue name is required.");

        if (string.IsNullOrWhiteSpace(request.Address))
            throw ApiException.BadRequest("Venue address is required.");

        if (request.TotalCapacity <= 0)
            throw ApiException.BadRequest(
                "Venue capacity must be greater than zero.");

        var name = request.Name.Trim();

        var existingVenue =
            await _venueRepository.GetByNameAsync(name);

        if (existingVenue is not null)
            throw ApiException.Conflict(
                "A venue with this name already exists.");

        var venue = new Venue
        {
            Name = name,
            Address = request.Address.Trim(),
            TotalCapacity = request.TotalCapacity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _venueRepository.AddAsync(venue);
        await _venueRepository.SaveChangesAsync();

        return MapToResponse(venue);
    }

    public async Task<VenueResponse> GetByIdAsync(
        int venueId)
    {
        var venue =
            await _venueRepository.GetByIdAsync(venueId)
            ?? throw ApiException.NotFound(
                "Venue not found.");

        return MapToResponse(venue);
    }

    public async Task<List<VenueResponse>> GetAllAsync()
    {
        var venues =
            await _venueRepository.GetAllAsync();

        return venues
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<VenueResponse> UpdateAsync(
        int venueId,
        UpdateVenueRequest request)
    {
        var venue =
            await _venueRepository.GetByIdAsync(venueId)
            ?? throw ApiException.NotFound(
                "Venue not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw ApiException.BadRequest(
                "Venue name is required.");

        if (string.IsNullOrWhiteSpace(request.Address))
            throw ApiException.BadRequest(
                "Venue address is required.");

        if (request.TotalCapacity <= 0)
            throw ApiException.BadRequest(
                "Venue capacity must be greater than zero.");

        var name = request.Name.Trim();

        var existingVenue =
            await _venueRepository.GetByNameAsync(name);

        if (existingVenue is not null &&
            existingVenue.VenueId != venueId)
        {
            throw ApiException.Conflict(
                "A venue with this name already exists.");
        }

        venue.Name = name;
        venue.Address = request.Address.Trim();
        venue.TotalCapacity = request.TotalCapacity;
        venue.UpdatedAt = DateTime.UtcNow;

        _venueRepository.Update(venue);

        await _venueRepository.SaveChangesAsync();

        return MapToResponse(venue);
    }

    public async Task DeleteAsync(
        int venueId)
    {
        var venue =
            await _venueRepository.GetByIdAsync(venueId)
            ?? throw ApiException.NotFound(
                "Venue not found.");

        // A venue with existing events should not be deleted.
        if (venue.Events.Any())
        {
            throw ApiException.Conflict(
                "Cannot delete a venue that has events.");
        }

        _venueRepository.Remove(venue);

        await _venueRepository.SaveChangesAsync();
    }

    private static VenueResponse MapToResponse(Venue venue)
    {
        return new VenueResponse(
            venue.VenueId,
            venue.Name,
            venue.Address,
            venue.TotalCapacity,
            venue.CreatedAt,
            venue.UpdatedAt);
    }
}