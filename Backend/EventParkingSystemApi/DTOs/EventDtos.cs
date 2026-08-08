namespace EventParkingSystemApi.DTOs
{
    public record EventResponse(
    int EventId, string Name, int VenueId, string VenueName, int CategoryId, string CategoryName,
    DateOnly EventDate, TimeOnly EventTime, decimal TicketPrice, decimal ParkingFee,
    string? Description, bool IsLocked, int TotalSeats, int AvailableSeats,
    int TotalParkingSlots, int AvailableParkingSlots);

    public record EventCreateRequest(
        string Name, int VenueId, int CategoryId, DateOnly EventDate, TimeOnly EventTime,
        decimal TicketPrice, decimal ParkingFee, string? Description,
        int SeatRows, int SeatsPerRow, int ParkingSlotCount);

    public record EventUpdateRequest(
        string Name, int VenueId, int CategoryId, DateOnly EventDate, TimeOnly EventTime,
        decimal TicketPrice, decimal ParkingFee, string? Description);

}
