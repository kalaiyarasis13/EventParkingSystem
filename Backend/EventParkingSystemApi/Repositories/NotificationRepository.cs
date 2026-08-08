using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;
        public NotificationRepository(AppDbContext db) => _db = db;

        public async Task<Notification?> GetByIdAsync(int id) => await _db.Notifications.FindAsync(id);

        public async Task AddAsync(Notification notification) => await _db.Notifications.AddAsync(notification);

        public void Update(Notification notification) => _db.Notifications.Update(notification);

        public void Remove(Notification notification) => _db.Notifications.Remove(notification);

        public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<List<Notification>> GetByCustomerOrderedAsync(int customerId) =>
            await _db.Notifications.Where(n => n.CustomerId == customerId)
                .OrderByDescending(n => n.CreatedAt).ToListAsync();

        public async Task<int> CountUnreadAsync(int customerId) =>
            await _db.Notifications.CountAsync(n => n.CustomerId == customerId && !n.IsRead);
    }
}
