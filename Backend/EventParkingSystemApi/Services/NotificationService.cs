using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;
using static EventParkingSystemApi.DTOs.NotificationDtos;

namespace EventParkingSystemApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationService(INotificationRepository notificationRepository) => _notificationRepository = notificationRepository;

        public async Task CreateAsync(int customerId, string title, string message, string type)
        {
            await _notificationRepository.AddAsync(new Notification
            {
                CustomerId = customerId,
                Title = title,
                Message = message,
                Type = type
            });
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task<List<NotificationResponse>> GetByCustomerAsync(int customerId)
        {
            // Rule: a customer should only ever see their own notifications (enforced by the customerId filter here;
            // the controller also checks the requester matches the route id or is an admin).
            var notifications = await _notificationRepository.GetByCustomerOrderedAsync(customerId);
            return notifications.Select(n => new NotificationResponse(n.NotificationId, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt)).ToList();
        }

        public async Task MarkAsReadAsync(int notificationId, int requestingCustomerId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId)
                ?? throw ApiException.NotFound("Notification not found.");

            if (notification.CustomerId != requestingCustomerId)
                throw ApiException.Forbidden("You can only manage your own notifications.");

            notification.IsRead = true;
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(int customerId) =>
            await _notificationRepository.CountUnreadAsync(customerId);
    }
}
