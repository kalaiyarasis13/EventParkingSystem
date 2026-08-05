namespace EventParkingSystemApi.Models
{
    public static class PaymentStatus
    {
        public const string Completed = "Completed";
    }
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; } = PaymentStatus.Completed;
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        public Booking? Booking { get; set; }
    }
}
