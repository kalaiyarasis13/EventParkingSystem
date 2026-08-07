using EventParkingSystemApi.IService;
using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;



namespace EventParkingSystemApi.Services;

public class PaymentService : IPaymentService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly INotificationService _notifications;

    public PaymentService(IBookingRepository bookingRepository, IPaymentRepository paymentRepository, INotificationService notifications)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _notifications = notifications;
    }

    public async Task<PaymentDueResponse> GetDueAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdWithFullDetailsAsync(bookingId)
            ?? throw ApiException.NotFound("Booking not found.");

        return new PaymentDueResponse(booking.BookingId, booking.BookingNumber, booking.TotalAmount, booking.Payment is not null);
    }

    // Rule 7: a booking cannot be marked Confirmed until payment completes.
    // Rule: a payment cannot be recorded twice for the same booking (enforced by unique index too).
    public async Task<PaymentResponse> PayAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdWithFullDetailsAsync(bookingId)
            ?? throw ApiException.NotFound("Booking not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw ApiException.Conflict("Cannot pay for a cancelled booking.");

        if (booking.Payment is not null)
            throw ApiException.Conflict("A payment has already been recorded for this booking.");

        using var transaction = await _paymentRepository.BeginTransactionAsync();
        try
        {
            var payment = new Payment { BookingId = bookingId, AmountPaid = booking.TotalAmount, Status = PaymentStatus.Completed };
            await _paymentRepository.AddAsync(payment);

            booking.Status = BookingStatus.Confirmed; // only flips after payment succeeds
            booking.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            await _notifications.CreateAsync(booking.CustomerId, "Payment Received",
                $"Payment of {payment.AmountPaid:C} for booking {booking.BookingNumber} was successful. Your booking is confirmed.",
                NotificationType.Confirmation);

            return new PaymentResponse(payment.PaymentId, booking.BookingId, booking.BookingNumber, payment.AmountPaid, payment.Status, payment.PaidAt);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            throw ApiException.Conflict("A payment has already been recorded for this booking.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<PaymentResponse>> GetByCustomerAsync(int customerId)
    {
        var payments = await _paymentRepository.GetByCustomerWithBookingAsync(customerId);
        return payments.Select(p => new PaymentResponse(p.PaymentId, p.BookingId, p.Booking!.BookingNumber, p.AmountPaid, p.Status, p.PaidAt)).ToList();
    }

    public async Task<ReceiptResponse> GetReceiptAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdWithReceiptDetailsAsync(paymentId)
            ?? throw ApiException.NotFound("Payment not found.");

        var booking = payment.Booking!;
        return new ReceiptResponse(
            booking.BookingNumber, booking.Customer!.FullName, booking.Event!.Name, booking.Event.EventDate,
            booking.BookingSeats.Select(bs => bs.Seat!.SeatRow + bs.Seat.SeatNumber).ToList(),
            booking.ParkingReservation?.ParkingSlot?.SlotLabel,
            payment.AmountPaid, payment.PaidAt);
    }
}
