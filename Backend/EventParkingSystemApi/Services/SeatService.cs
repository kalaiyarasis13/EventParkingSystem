using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.IService;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.Services;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seatRepository;
    private readonly IEventRepository _eventRepository;

    public SeatService(
        ISeatRepository seatRepository,
        IEventRepository eventRepository)
    {
        _seatRepository = seatRepository;
        _eventRepository = eventRepository;
    }

    public async Task<SeatResponse> CreateAsync(
        CreateSeatRequest request)
    {
        _ = await _eventRepository.GetByIdAsync(request.EventId)
            ?? throw ApiException.NotFound("Event not found.");

        if (string.IsNullOrWhiteSpace(request.SeatRow))
            throw ApiException.BadRequest("Seat row is required.");

        if (request.SeatNumber <= 0)
            throw ApiException.BadRequest(
                "Seat number must be greater than zero.");

        var seatRow = request.SeatRow.Trim().ToUpper();

        var existingSeats =
            await _seatRepository.GetByEventAsync(request.EventId);

        if (existingSeats.Any(s =>
            s.SeatRow.Equals(
                seatRow,
                StringComparison.OrdinalIgnoreCase) &&
            s.SeatNumber == request.SeatNumber))
        {
            throw ApiException.Conflict(
                "This seat already exists for this event.");
        }

        var seat = new Seat
        {
            EventId = request.EventId,
            SeatRow = seatRow,
            SeatNumber = request.SeatNumber,
            Status = SeatStatus.Available
        };

        await _seatRepository.AddAsync(seat);
        await _seatRepository.SaveChangesAsync();

        return MapToResponse(seat);
    }

    public async Task<SeatResponse> GetByIdAsync(
        int seatId)
    {
        var seat = await _seatRepository.GetByIdAsync(seatId)
            ?? throw ApiException.NotFound("Seat not found.");

        return MapToResponse(seat);
    }

    public async Task<List<SeatResponse>> GetByEventAsync(
        int eventId)
    {
        _ = await _eventRepository.GetByIdAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        var seats =
            await _seatRepository.GetByEventAsync(eventId);

        return seats
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<SeatResponse>> GetAvailableByEventAsync(
        int eventId)
    {
        _ = await _eventRepository.GetByIdAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        var seats =
            await _seatRepository.GetAvailableByEventAsync(eventId);

        return seats
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<SeatResponse> UpdateAsync(
        int seatId,
        UpdateSeatRequest request)
    {
        var seat = await _seatRepository.GetByIdAsync(seatId)
            ?? throw ApiException.NotFound("Seat not found.");

        if (string.IsNullOrWhiteSpace(request.SeatRow))
            throw ApiException.BadRequest("Seat row is required.");

        if (request.SeatNumber <= 0)
            throw ApiException.BadRequest(
                "Seat number must be greater than zero.");

        var seatRow = request.SeatRow.Trim().ToUpper();

        var existingSeats =
            await _seatRepository.GetByEventAsync(seat.EventId);

        if (existingSeats.Any(s =>
            s.SeatId != seatId &&
            s.SeatRow.Equals(
                seatRow,
                StringComparison.OrdinalIgnoreCase) &&
            s.SeatNumber == request.SeatNumber))
        {
            throw ApiException.Conflict(
                "This seat already exists for this event.");
        }

        // Don't modify the booking status during a normal update.
        seat.SeatRow = seatRow;
        seat.SeatNumber = request.SeatNumber;

        _seatRepository.Update(seat);
        await _seatRepository.SaveChangesAsync();

        return MapToResponse(seat);
    }

    public async Task DeleteAsync(int seatId)
    {
        var seat = await _seatRepository.GetByIdAsync(seatId)
            ?? throw ApiException.NotFound("Seat not found.");

        if (seat.Status == SeatStatus.Booked)
        {
            throw ApiException.Conflict(
                "Cannot delete a booked seat.");
        }

        if (seat.BookingSeat is not null)
        {
            throw ApiException.Conflict(
                "Cannot delete a seat that is associated with a booking.");
        }

        _seatRepository.Remove(seat);
        await _seatRepository.SaveChangesAsync();
    }

    private static SeatResponse MapToResponse(Seat seat)
    {
        return new SeatResponse(
            seat.SeatId,
            seat.EventId,
            seat.SeatRow,
            seat.SeatNumber,
            seat.Status);
    }
}