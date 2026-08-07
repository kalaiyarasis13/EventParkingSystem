namespace EventParkingSystemApi.DTOs;

public record PaymentDueResponse(int BookingId, string BookingNumber, decimal AmountDue, bool IsPaid);
public record PaymentResponse(int PaymentId, int BookingId, string BookingNumber, decimal AmountPaid, string Status, DateTime PaidAt);
public record ReceiptResponse(
    string BookingNumber, string CustomerName, string EventName, DateOnly EventDate,
    List<string> SeatLabels, string? ParkingSlotLabel, decimal AmountPaid, DateTime PaidAt);
