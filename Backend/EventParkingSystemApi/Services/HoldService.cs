using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.Services;

public class HoldService : IHoldService
{
    private readonly IHoldRepository _holdRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IEventRepository _eventRepository;

    public HoldService(
        IHoldRepository holdRepository,
        ISeatRepository seatRepository,
        IEventRepository eventRepository)
    {
        _holdRepository = holdRepository;
        _seatRepository = seatRepository;
        _eventRepository = eventRepository;
    }

    public async Task<HoldResponse> CreateAsync(
        int customerId,
        CreateHoldRequest request)
    {
        // Check customer is selecting a valid event
        var ev = await _eventRepository.GetByIdAsync(request.EventId)
            ?? throw ApiException.NotFound("Event not found.");

        // Check seat belongs to this event
        var seats = await _seatRepository.GetByIdsForEventAsync(
            new List<int> { request.SeatId },
            request.EventId);

        var seat = seats.FirstOrDefault();

        if (seat is null)
            throw ApiException.BadRequest(
                "The selected seat does not belong to this event.");

        // Seat already booked
        if (seat.Status == SeatStatus.Booked)
            throw ApiException.Conflict(
                "The selected seat is already booked.");

        // Check existing active hold
        var existingHold =
            await _holdRepository.GetActiveHoldBySeatAsync(request.SeatId);

        if (existingHold is not null)
        {
            if (existingHold.CustomerId == customerId)
            {
                throw ApiException.Conflict(
                    "You already have an active hold for this seat.");
            }

            throw ApiException.Conflict(
                "The selected seat is currently held by another customer.");
        }

        // Create temporary hold
        var hold = new SeatHold
        {
            SeatId = request.SeatId,
            EventId = request.EventId,
            CustomerId = customerId,
            HeldAt = DateTime.UtcNow,

            // Hold seat for 10 minutes
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),

            Status = SeatHoldStatus.Active
        };

        await _holdRepository.AddAsync(hold);
        await _holdRepository.SaveChangesAsync();

        return MapToResponse(hold);
    }

    public async Task<HoldResponse> GetByIdAsync(int holdId)
    {
        var hold = await _holdRepository.GetByIdAsync(holdId)
            ?? throw ApiException.NotFound("Hold not found.");

        // Automatically mark expired hold
        if (hold.Status == SeatHoldStatus.Active &&
            hold.ExpiresAt <= DateTime.UtcNow)
        {
            hold.Status = SeatHoldStatus.Expired;

            _holdRepository.Update(hold);
            await _holdRepository.SaveChangesAsync();
        }

        return MapToResponse(hold);
    }

    public async Task<List<HoldResponse>> GetMyActiveHoldsAsync(
        int customerId)
    {
        var holds =
            await _holdRepository.GetActiveHoldsByCustomerAsync(customerId);

        return holds
            .Select(MapToResponse)
            .ToList();
    }

    public async Task ReleaseAsync(
        int holdId,
        int customerId)
    {
        var hold = await _holdRepository.GetByIdAsync(holdId)
            ?? throw ApiException.NotFound("Hold not found.");

        // Only owner can release the hold
        if (hold.CustomerId != customerId)
            throw ApiException.Forbidden(
                "You can only release your own seat hold.");

        if (hold.Status != SeatHoldStatus.Active)
            throw ApiException.Conflict(
                "This seat hold is no longer active.");

        hold.Status = SeatHoldStatus.Released;

        _holdRepository.Update(hold);

        await _holdRepository.SaveChangesAsync();
    }

    public async Task ExpireAsync(int holdId)
    {
        var hold = await _holdRepository.GetByIdAsync(holdId)
            ?? throw ApiException.NotFound("Hold not found.");

        if (hold.Status != SeatHoldStatus.Active)
            throw ApiException.Conflict(
                "This seat hold is no longer active.");

        if (hold.ExpiresAt > DateTime.UtcNow)
            throw ApiException.Conflict(
                "This seat hold has not expired yet.");

        hold.Status = SeatHoldStatus.Expired;

        _holdRepository.Update(hold);

        await _holdRepository.SaveChangesAsync();
    }

    private static HoldResponse MapToResponse(SeatHold hold)
    {
        return new HoldResponse(
            hold.HoldId,
            hold.SeatId,
            hold.EventId,
            hold.CustomerId,
            hold.HeldAt,
            hold.ExpiresAt,
            hold.Status);
    }
}