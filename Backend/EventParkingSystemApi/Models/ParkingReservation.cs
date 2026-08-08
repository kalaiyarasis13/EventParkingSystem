namespace EventParkingSystemApi.Models
{
    public class ParkingReservation
    {
        public int ParkingReservationId { get; set; }
        public int BookingId { get; set; }
        public int ParkingSlotId { get; set; }
        public decimal FeeAtBooking { get; set; }
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        public Booking? Booking { get; set; }
        public ParkingSlot? ParkingSlot { get; set; }
    }
}
