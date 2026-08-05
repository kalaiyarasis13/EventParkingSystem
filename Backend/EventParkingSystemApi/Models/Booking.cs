using Microsoft.Extensions.Logging;

namespace EventParkingSystemApi.Models
{

    public static class BookingStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Cancelled = "Cancelled";
    }

    public class Booking
    {
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int EventId { get; set; }
        public string Status { get; set; } = BookingStatus.Pending;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Customer? Customer { get; set; }
        public Event? Event { get; set; }
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
        public ParkingReservation? ParkingReservation { get; set; }
        public Payment? Payment { get; set; }
    }
}
