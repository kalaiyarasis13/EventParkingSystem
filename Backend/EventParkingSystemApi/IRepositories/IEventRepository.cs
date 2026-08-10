using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepositories
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id);
        Task AddAsync(Event ev);
        void Update(Event ev);
        void Remove(Event ev);
        Task<int> SaveChangesAsync();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();

        Task<List<Event>> SearchAsync(string? name, DateOnly? date, int? venueId, int? categoryId);
        Task<Event?> GetByIdWithDetailsAsync(int id);
        Task<bool> HasUpcomingEventsForVenueAsync(int venueId);
        Task<int> CountAsync();
    }
}
