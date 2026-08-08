using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventParkingSystemApi.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;
        public CustomerRepository(AppDbContext db) => _db = db;

        public async Task<Customer?> GetByIdAsync(int id) => await _db.Customers.FindAsync(id);

        public async Task AddAsync(Customer customer) => await _db.Customers.AddAsync(customer);

        public void Update(Customer customer) => _db.Customers.Update(customer);

        public void Remove(Customer customer) => _db.Customers.Remove(customer);

        public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync() => await _db.Database.BeginTransactionAsync();

        public async Task<Customer?> GetByEmailAsync(string email) =>
            await _db.Customers.FirstOrDefaultAsync(c => c.Email == email);

        public async Task<bool> ExistsByEmailAsync(string email) =>
            await _db.Customers.AnyAsync(c => c.Email == email);

        public async Task<List<Customer>> SearchAsync(string? search)
        {
            var query = _db.Customers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.FullName.Contains(search) || c.Email.Contains(search));

            return await query.OrderBy(c => c.FullName).ToListAsync();
        }

        public async Task<int> CountByRoleAsync(string role) =>
            await _db.Customers.CountAsync(c => c.Role == role);
    }
}
