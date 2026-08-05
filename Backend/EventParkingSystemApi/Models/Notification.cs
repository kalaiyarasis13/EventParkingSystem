namespace EventParkingSystemApi.Models
{
    public static class NotificationType
    {
        public const string Confirmation = "Confirmation";
        public const string Cancellation = "Cancellation";
        public const string Reminder = "Reminder";
        public const string Update = "Update";
    }
    public class Notification
    {
        public int NotificationId { get; set; }
        public int CustomerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Customer? Customer { get; set; }
    }
}
