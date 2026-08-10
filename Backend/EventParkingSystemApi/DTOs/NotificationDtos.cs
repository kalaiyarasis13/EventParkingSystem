namespace EventParkingSystemApi.DTOs
{
    public class NotificationDtos
    {
        public record NotificationResponse(int NotificationId, string Title, string Message, string Type, bool IsRead, DateTime CreatedAt);
    }
}
