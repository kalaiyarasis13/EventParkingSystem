namespace EventParkingSystemApi.DTOs;

public record CreateBookingSeatRequest(
    int BookingId,
    int SeatId,
    decimal PriceAtBooking
);

public record UpdateBookingSeatRequest(
    decimal PriceAtBooking
);