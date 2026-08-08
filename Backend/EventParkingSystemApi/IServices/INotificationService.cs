using static EventParkingSystemApi.DTOs.NotificationDtos;

namespace EventParkingSystemApi.IServices
{
    public interface INotificationService
    {
        Task CreateAsync(int customerId, string title, string message, string type);
        Task<List<NotificationResponse>> GetByCustomerAsync(int customerId);
        Task MarkAsReadAsync(int notificationId, int requestingCustomerId);
        Task<int> GetUnreadCountAsync(int customerId);
    }
}
