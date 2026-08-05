namespace EventParkingSystemApi.Models
{
    public static class ParkingSlotStatus
    {
        public const string Available = "Available";
        public const string Occupied = "Occupied";
    }
    public class ParkingSlot
    {
        public int ParkingSlotId { get; set; }
        public int EventId { get; set; }
        public string SlotLabel { get; set; } = string.Empty;
        public string Status { get; set; } = ParkingSlotStatus.Available;

        public Event? Event { get; set; }
        public ParkingReservation? ParkingReservation { get; set; }
    }
}
