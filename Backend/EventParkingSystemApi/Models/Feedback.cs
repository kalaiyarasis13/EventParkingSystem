namespace EventParkingSystemApi.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        public int BookingId { get; set; }

        public int CustomerId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Booking? Booking { get; set; }

        public Customer? Customer { get; set; }
    }
}