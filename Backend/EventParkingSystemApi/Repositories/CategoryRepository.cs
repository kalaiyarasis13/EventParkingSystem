using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventParkingSystemApi.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _db;
        public CategoryRepository(AppDbContext db) => _db = db;

        public async Task<EventCategory?> GetByIdAsync(int id) => await _db.EventCategories.FindAsync(id);

        public async Task AddAsync(EventCategory category) => await _db.EventCategories.AddAsync(category);

        public void Update(EventCategory category) => _db.EventCategories.Update(category);

        public void Remove(EventCategory category) => _db.EventCategories.Remove(category);

        public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync() => await _db.Database.BeginTransactionAsync();

        public async Task<List<EventCategory>> GetAllOrderedAsync() =>
            await _db.EventCategories.OrderBy(c => c.Name).ToListAsync();

        public async Task<bool> ExistsAsync(int categoryId) =>
            await _db.EventCategories.AnyAsync(c => c.CategoryId == categoryId);

        public async Task<bool> ExistsByNameAsync(string name) =>
            await _db.EventCategories.AnyAsync(c => c.Name == name);
    }
}
