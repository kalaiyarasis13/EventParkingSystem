using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.IService;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Services;

public class ParkingReservationService : IParkingReservationService
{
    private readonly IParkingReservationRepository _parkingReservationRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IParkingSlotRepository _parkingSlotRepository;

    public ParkingReservationService(
        IParkingReservationRepository parkingReservationRepository,
        IBookingRepository bookingRepository,
        IParkingSlotRepository parkingSlotRepository)
    {
        _parkingReservationRepository = parkingReservationRepository;
        _bookingRepository = bookingRepository;
        _parkingSlotRepository = parkingSlotRepository;
    }

    public async Task<ParkingReservationResponse> CreateAsync(
        int customerId,
        CreateParkingReservationRequest request)
    {
        // Check booking
        var booking = await _bookingRepository.GetByIdWithFullDetailsAsync(
            request.BookingId)
            ?? throw ApiException.NotFound("Booking not found.");

        // Customer can only use their own booking
        if (booking.CustomerId != customerId)
            throw ApiException.Forbidden(
                "You can only reserve parking for your own booking.");

        // Cancelled booking cannot have parking
        if (booking.Status == BookingStatus.Cancelled)
            throw ApiException.Conflict(
                "Cannot reserve parking for a cancelled booking.");

        // One parking reservation per booking
        var existingReservation =
            await _parkingReservationRepository.GetByBookingIdAsync(
                request.BookingId);

        if (existingReservation is not null)
            throw ApiException.Conflict(
                "This booking already has a parking reservation.");

        // Check parking slot belongs to the booking's event
        var slot = await _parkingSlotRepository.GetByIdForEventAsync(
            request.ParkingSlotId,
            booking.EventId);

        if (slot is null)
            throw ApiException.BadRequest(
                "The selected parking slot does not belong to this event.");

        // Check slot availability
        if (slot.Status == ParkingSlotStatus.Occupied)
            throw ApiException.Conflict(
                "The selected parking slot is already occupied.");

        // Check whether slot has an existing reservation
        var slotReservation =
            await _parkingReservationRepository.GetByParkingSlotIdAsync(
                request.ParkingSlotId);

        if (slotReservation is not null)
            throw ApiException.Conflict(
                "The selected parking slot is already reserved.");

        // Create reservation
        var reservation = new ParkingReservation
        {
            BookingId = booking.BookingId,
            ParkingSlotId = slot.ParkingSlotId,
            FeeAtBooking = booking.Event!.ParkingFee,
            ReservedAt = DateTime.UtcNow
        };

        slot.Status = ParkingSlotStatus.Occupied;

        await _parkingReservationRepository.AddAsync(reservation);

        try
        {
            await _parkingReservationRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw ApiException.Conflict(
                "The selected parking slot was just reserved by another customer.");
        }

        return MapToResponse(reservation, slot.SlotLabel);
    }

    public async Task<ParkingReservationResponse> GetByIdAsync(
        int parkingReservationId)
    {
        var reservation =
            await _parkingReservationRepository.GetByIdAsync(
                parkingReservationId)
            ?? throw ApiException.NotFound(
                "Parking reservation not found.");

        return MapToResponse(
            reservation,
            reservation.ParkingSlot?.SlotLabel ?? "");
    }

    public async Task<ParkingReservationResponse?> GetByBookingIdAsync(
        int bookingId)
    {
        var reservation =
            await _parkingReservationRepository.GetByBookingIdAsync(
                bookingId);

        if (reservation is null)
            return null;

        return MapToResponse(
            reservation,
            reservation.ParkingSlot?.SlotLabel ?? "");
    }

    public async Task<List<ParkingReservationResponse>> GetMyReservationsAsync(
        int customerId)
    {
        var reservations =
            await _parkingReservationRepository.GetByCustomerAsync(
                customerId);

        return reservations
            .Select(r => MapToResponse(
                r,
                r.ParkingSlot?.SlotLabel ?? ""))
            .ToList();
    }

    public async Task CancelAsync(
        int parkingReservationId,
        int customerId)
    {
        var reservation =
            await _parkingReservationRepository.GetByIdAsync(
                parkingReservationId)
            ?? throw ApiException.NotFound(
                "Parking reservation not found.");

        if (reservation.Booking is null)
            throw ApiException.NotFound("Booking not found.");

        if (reservation.Booking.CustomerId != customerId)
            throw ApiException.Forbidden(
                "You can only cancel your own parking reservation.");

        // Release parking slot
        if (reservation.ParkingSlot is not null)
        {
            reservation.ParkingSlot.Status =
                ParkingSlotStatus.Available;
        }

        _parkingReservationRepository.Remove(reservation);

        await _parkingReservationRepository.SaveChangesAsync();
    }

    private static ParkingReservationResponse MapToResponse(
        ParkingReservation reservation,
        string slotLabel)
    {
        return new ParkingReservationResponse(
            reservation.ParkingReservationId,
            reservation.BookingId,
            reservation.ParkingSlotId,
            slotLabel,
            reservation.FeeAtBooking,
            reservation.ReservedAt);
    }
}