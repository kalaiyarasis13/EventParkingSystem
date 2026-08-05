namespace EventParkingSystemApi.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int VenueId { get; set; }
        public int CategoryId { get; set; }
        public DateOnly EventDate { get; set; }
        public TimeOnly EventTime { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal ParkingFee { get; set; }
        public string? Description { get; set; }
        public bool IsLocked { get; set; } = false; // frozen once bookings exist
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Venue? Venue { get; set; }
        public EventCategory? Category { get; set; }
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<ParkingSlot> ParkingSlots { get; set; } = new List<ParkingSlot>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
