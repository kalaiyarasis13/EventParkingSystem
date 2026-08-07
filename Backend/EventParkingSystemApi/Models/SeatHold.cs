namespace EventParkingSystemApi.Models;

public static class SeatHoldStatus
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Released = "Released";
}

public class SeatHold
{
    public int HoldId { get; set; }

    public int SeatId { get; set; }

    public int EventId { get; set; }

    public int CustomerId { get; set; }

    public DateTime HeldAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public string Status { get; set; } = SeatHoldStatus.Active;

    // Navigation properties
    public Seat? Seat { get; set; }

    public Event? Event { get; set; }

    public Customer? Customer { get; set; }
}