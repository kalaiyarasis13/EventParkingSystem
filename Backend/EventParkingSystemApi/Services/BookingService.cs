using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;


namespace EventParkingSystemApi.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IParkingSlotRepository _parkingSlotRepository;
    private readonly IBookingSeatRepository _bookingSeatRepository;
    private readonly IParkingReservationRepository _parkingReservationRepository;
    private readonly INotificationService _notifications;

    public BookingService(
        IBookingRepository bookingRepository, ICustomerRepository customerRepository, IEventRepository eventRepository,
        ISeatRepository seatRepository, IParkingSlotRepository parkingSlotRepository,
        IBookingSeatRepository bookingSeatRepository, IParkingReservationRepository parkingReservationRepository,
        INotificationService notifications)
    {
        _bookingRepository = bookingRepository;
        _customerRepository = customerRepository;
        _eventRepository = eventRepository;
        _seatRepository = seatRepository;
        _parkingSlotRepository = parkingSlotRepository;
        _bookingSeatRepository = bookingSeatRepository;
        _parkingReservationRepository = parkingReservationRepository;
        _notifications = notifications;
    }

    // Combines seat selection + optional parking into a single booking (Module 6),
    // reusing the same double-booking-safe logic as Modules 4 & 5, all inside one transaction.
    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request)
    {
        // Rule 5: A booking must contain at least one seat.
        if (request.SeatIds == null || request.SeatIds.Count == 0)
            throw ApiException.BadRequest("A booking must contain at least one seat.");

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId)
            ?? throw ApiException.NotFound("Customer not found.");
        var ev = await _eventRepository.GetByIdAsync(request.EventId)
            ?? throw ApiException.NotFound("Event not found.");

        using var transaction = await _bookingRepository.BeginTransactionAsync();
        try
        {
            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                EventId = request.EventId,
                Status = BookingStatus.Pending,
                BookingNumber = "" // set after we have an Id
            };
            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync(); // generates BookingId

            booking.BookingNumber = BookingNumberGenerator.Generate(booking.BookingId, DateTime.UtcNow.Year);

            // --- Seats (Rules 1 & 3: no double-booking, re-checked here inside the transaction) ---
            var seats = await _seatRepository.GetByIdsForEventAsync(request.SeatIds, request.EventId);
            if (seats.Count != request.SeatIds.Count)
                throw ApiException.BadRequest("One or more selected seats do not belong to this event.");

            var alreadyBooked = seats.Where(s => s.Status == SeatStatus.Booked).ToList();
            if (alreadyBooked.Any())
                throw ApiException.Conflict($"Seat(s) {string.Join(", ", alreadyBooked.Select(s => s.SeatRow + s.SeatNumber))} were just taken. Please choose different seats.");

            decimal total = 0m;
            foreach (var seat in seats)
            {
                seat.Status = SeatStatus.Booked;
                await _bookingSeatRepository.AddAsync(new BookingSeat { BookingId = booking.BookingId, SeatId = seat.SeatId, PriceAtBooking = ev.TicketPrice });
                total += ev.TicketPrice;
            }

            // --- Parking (Rule: optional; Rule 2: one slot per customer per event) ---
            if (request.ParkingSlotId.HasValue)
            {
                var slot = await _parkingSlotRepository.GetByIdForEventAsync(request.ParkingSlotId.Value, request.EventId)
                    ?? throw ApiException.BadRequest("The selected parking slot does not belong to this event.");

                if (slot.Status == ParkingSlotStatus.Occupied)
                    throw ApiException.Conflict("The selected parking slot was just taken. Please choose another or continue without parking.");

                slot.Status = ParkingSlotStatus.Occupied;
                await _parkingReservationRepository.AddAsync(new ParkingReservation { BookingId = booking.BookingId, ParkingSlotId = slot.ParkingSlotId, FeeAtBooking = ev.ParkingFee });
                total += ev.ParkingFee;
            }

            booking.TotalAmount = total;
           // ev.IsLocked = true; // seat map / pricing now frozen for this event

            // All repositories above share the same scoped DbContext, so this single call
            // persists the booking, its seats, and its parking reservation together.
            await _bookingRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            await _notifications.CreateAsync(customer.CustomerId, "Booking Created",
                $"Your booking {booking.BookingNumber} for {ev.Name} has been created. Complete payment to confirm.",
                NotificationType.Confirmation);

            return await GetByIdAsync(booking.BookingId);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            throw ApiException.Conflict("A seat or parking slot in your selection was just taken by another customer.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<BookingResponse>> GetByCustomerAsync(int customerId)
    {
        var bookings = await _bookingRepository.GetByCustomerWithDetailsAsync(customerId);
        return bookings.Select(MapToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetByEventAsync(int eventId)
    {
        var bookings = await _bookingRepository.GetByEventWithDetailsAsync(eventId);
        return bookings.Select(MapToResponse).ToList();
    }

    public async Task<BookingResponse> GetByIdAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdWithFullDetailsAsync(id)
            ?? throw ApiException.NotFound("Booking not found.");
        return MapToResponse(booking);
    }

    // Cancelling releases all seats and any parking slot back to Available.
    public async Task CancelAsync(int id, int requestingCustomerId, bool isAdmin)
    {
        var booking = await _bookingRepository.GetByIdWithFullDetailsAsync(id)
            ?? throw ApiException.NotFound("Booking not found.");

        if (!isAdmin && booking.CustomerId != requestingCustomerId)
            throw ApiException.Forbidden("You can only cancel your own bookings.");

        if (booking.Status == BookingStatus.Cancelled)
            throw ApiException.Conflict("This booking is already cancelled.");

        using var transaction = await _bookingRepository.BeginTransactionAsync();
        try
        {
            foreach (var bs in booking.BookingSeats.ToList())
            {
                bs.Seat!.Status = SeatStatus.Available;

                _bookingSeatRepository.Remove(bs);
            }

            if (booking.ParkingReservation is not null)
            {
                booking.ParkingReservation.ParkingSlot!.Status =
                    ParkingSlotStatus.Available;

                _parkingReservationRepository.Remove(
                    booking.ParkingReservation
                );
            }

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            await _notifications.CreateAsync(booking.CustomerId, "Booking Cancelled",
                $"Your booking {booking.BookingNumber} for {booking.Event!.Name} has been cancelled and seats/parking released.",
                NotificationType.Cancellation);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static BookingResponse MapToResponse(Booking b) => new(
        b.BookingId, b.BookingNumber, b.CustomerId, b.Customer?.FullName ?? "",
        b.EventId, b.Event?.Name ?? "", b.Event?.EventDate ?? default, b.Event?.EventTime ?? default,
        b.Status, b.TotalAmount, b.CreatedAt,
        b.BookingSeats.Select(bs => new BookingSeatResponse(bs.SeatId, bs.Seat!.SeatRow + bs.Seat.SeatNumber, bs.PriceAtBooking)).ToList(),
        b.ParkingReservation is null ? null : new BookingParkingResponse(b.ParkingReservation.ParkingSlotId, b.ParkingReservation.ParkingSlot!.SlotLabel, b.ParkingReservation.FeeAtBooking),
        b.Payment is not null);
}
