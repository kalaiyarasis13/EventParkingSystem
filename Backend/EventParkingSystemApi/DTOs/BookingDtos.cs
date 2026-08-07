namespace EventParkingSystemApi.DTOs;

public record CreateBookingRequest(int CustomerId, int EventId, List<int> SeatIds, int? ParkingSlotId);

public record BookingSeatResponse(int SeatId, string SeatLabel, decimal PriceAtBooking);
public record BookingParkingResponse(int ParkingSlotId, string SlotLabel, decimal FeeAtBooking);

public record BookingResponse(
    int BookingId, string BookingNumber, int CustomerId, string CustomerName,
    int EventId, string EventName, DateOnly EventDate, TimeOnly EventTime,
    string Status, decimal TotalAmount, DateTime CreatedAt,
    List<BookingSeatResponse> Seats, BookingParkingResponse? Parking, bool IsPaid);
