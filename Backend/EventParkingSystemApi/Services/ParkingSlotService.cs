using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.Services;

public class ParkingSlotService : IParkingSlotService
{
    private readonly IParkingSlotRepository _parkingSlotRepository;
    private readonly IEventRepository _eventRepository;

    public ParkingSlotService(
        IParkingSlotRepository parkingSlotRepository,
        IEventRepository eventRepository)
    {
        _parkingSlotRepository = parkingSlotRepository;
        _eventRepository = eventRepository;
    }

    public async Task<ParkingSlotResponse> CreateAsync(
        CreateParkingSlotRequest request)
    {
        // Check event exists
        var ev = await _eventRepository.GetByIdAsync(request.EventId)
            ?? throw ApiException.NotFound("Event not found.");

        if (string.IsNullOrWhiteSpace(request.SlotLabel))
            throw ApiException.BadRequest(
                "Parking slot label is required.");

        var slotLabel = request.SlotLabel.Trim().ToUpper();

        // Check duplicate slot label for same event
        var existingSlots =
            await _parkingSlotRepository.GetByEventAsync(request.EventId);

        if (existingSlots.Any(s =>
            s.SlotLabel.Equals(
                slotLabel,
                StringComparison.OrdinalIgnoreCase)))
        {
            throw ApiException.Conflict(
                "A parking slot with this label already exists for this event.");
        }

        var parkingSlot = new ParkingSlot
        {
            EventId = request.EventId,
            SlotLabel = slotLabel,
            Status = ParkingSlotStatus.Available
        };

        await _parkingSlotRepository.AddAsync(parkingSlot);
        await _parkingSlotRepository.SaveChangesAsync();

        return MapToResponse(parkingSlot);
    }

    public async Task<ParkingSlotResponse> GetByIdAsync(
        int parkingSlotId)
    {
        var parkingSlot =
            await _parkingSlotRepository.GetByIdAsync(parkingSlotId)
            ?? throw ApiException.NotFound(
                "Parking slot not found.");

        return MapToResponse(parkingSlot);
    }

    public async Task<List<ParkingSlotResponse>> GetByEventAsync(
        int eventId)
    {
        // Check event exists
        _ = await _eventRepository.GetByIdAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        var slots =
            await _parkingSlotRepository.GetByEventAsync(eventId);

        return slots
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<ParkingSlotResponse>> GetAvailableByEventAsync(
        int eventId)
    {
        // Check event exists
        _ = await _eventRepository.GetByIdAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        var slots =
            await _parkingSlotRepository.GetAvailableByEventAsync(eventId);

        return slots
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ParkingSlotResponse> UpdateAsync(
        int parkingSlotId,
        UpdateParkingSlotRequest request)
    {
        var parkingSlot =
            await _parkingSlotRepository.GetByIdAsync(parkingSlotId)
            ?? throw ApiException.NotFound(
                "Parking slot not found.");

        if (string.IsNullOrWhiteSpace(request.SlotLabel))
            throw ApiException.BadRequest(
                "Parking slot label is required.");

        var slotLabel = request.SlotLabel.Trim().ToUpper();

        // Check duplicate label
        var eventSlots =
            await _parkingSlotRepository.GetByEventAsync(
                parkingSlot.EventId);

        if (eventSlots.Any(s =>
            s.ParkingSlotId != parkingSlotId &&
            s.SlotLabel.Equals(
                slotLabel,
                StringComparison.OrdinalIgnoreCase)))
        {
            throw ApiException.Conflict(
                "A parking slot with this label already exists for this event.");
        }

        // Do not change occupied slot unnecessarily
        parkingSlot.SlotLabel = slotLabel;

        _parkingSlotRepository.Update(parkingSlot);

        await _parkingSlotRepository.SaveChangesAsync();

        return MapToResponse(parkingSlot);
    }

    public async Task DeleteAsync(
        int parkingSlotId)
    {
        var parkingSlot =
            await _parkingSlotRepository.GetByIdAsync(parkingSlotId)
            ?? throw ApiException.NotFound(
                "Parking slot not found.");

        // Do not delete occupied/reserved slot
        if (parkingSlot.Status == ParkingSlotStatus.Occupied ||
            parkingSlot.ParkingReservation is not null)
        {
            throw ApiException.Conflict(
                "Cannot delete a parking slot that is currently reserved.");
        }

        _parkingSlotRepository.Remove(parkingSlot);

        await _parkingSlotRepository.SaveChangesAsync();
    }

    private static ParkingSlotResponse MapToResponse(
        ParkingSlot parkingSlot)
    {
        return new ParkingSlotResponse(
            parkingSlot.ParkingSlotId,
            parkingSlot.EventId,
            parkingSlot.SlotLabel,
            parkingSlot.Status);
    }
}