using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;

namespace EventParkingSystemApi.Services;

public class BookingSeatService : IBookingSeatService
{
    private readonly IBookingSeatRepository _bookingSeatRepository;

    public BookingSeatService(
        IBookingSeatRepository bookingSeatRepository)
    {
        _bookingSeatRepository = bookingSeatRepository;
    }

    public async Task<List<BookingSeatResponse>> GetByBookingIdAsync(
        int bookingId)
    {
        var bookingSeats =
            await _bookingSeatRepository.GetByBookingIdAsync(bookingId);

        return bookingSeats
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<BookingSeatResponse> GetByIdAsync(
        int bookingSeatId)
    {
        var bookingSeat =
            await _bookingSeatRepository.GetByIdAsync(bookingSeatId)
            ?? throw ApiException.NotFound(
                "Booking seat not found.");

        return MapToResponse(bookingSeat);
    }

    public async Task DeleteAsync(int bookingSeatId)
    {
        var bookingSeat =
            await _bookingSeatRepository.GetByIdAsync(bookingSeatId)
            ?? throw ApiException.NotFound(
                "Booking seat not found.");

        _bookingSeatRepository.Remove(bookingSeat);

        await _bookingSeatRepository.SaveChangesAsync();
    }

    private static BookingSeatResponse MapToResponse(
        Models.BookingSeat bookingSeat)
    {
        return new BookingSeatResponse(
            bookingSeat.SeatId,
            bookingSeat.Seat?.SeatRow +
                bookingSeat.Seat?.SeatNumber,
            bookingSeat.PriceAtBooking);
    }
}