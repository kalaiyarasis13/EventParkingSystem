using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepositories
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(int id);
        Task AddAsync(Notification notification);
        void Update(Notification notification);
        void Remove(Notification notification);
        Task<int> SaveChangesAsync();

        Task<List<Notification>> GetByCustomerOrderedAsync(int customerId);
        Task<int> CountUnreadAsync(int customerId);
    }
}
