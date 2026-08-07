namespace EventParkingSystemApi.Models
{
    public static class SeatStatus
    {
        public const string Available = "Available";
        public const string Booked = "Booked";
    }
    public class Seat
    {
        public int SeatId { get; set; }
        public int EventId { get; set; }
        public string SeatRow { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public string Status { get; set; } = SeatStatus.Available;

        public Event? Event { get; set; }
        public BookingSeat? BookingSeat { get; set; }

        public string SeatLabel => $"{SeatRow}{SeatNumber}";

        public ICollection<SeatHold> SeatHolds { get; set; }
       = new List<SeatHold>();
    }
}
