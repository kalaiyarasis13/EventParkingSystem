using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepositories
{
    public interface ICategoryRepository
    {
        Task<EventCategory?> GetByIdAsync(int id);
        Task AddAsync(EventCategory category);
        void Update(EventCategory category);
        void Remove(EventCategory category);
        Task<int> SaveChangesAsync();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();

        Task<List<EventCategory>> GetAllOrderedAsync();
        Task<bool> ExistsAsync(int categoryId);
        Task<bool> ExistsByNameAsync(string name);
    }
}
