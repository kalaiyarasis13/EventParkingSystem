using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventParkingSystemApi.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _db;
        public EventRepository(AppDbContext db) => _db = db;

        public async Task<Event?> GetByIdAsync(int id) => await _db.Events.FindAsync(id);

        public async Task AddAsync(Event ev) => await _db.Events.AddAsync(ev);

        public void Update(Event ev) => _db.Events.Update(ev);

        public void Remove(Event ev) => _db.Events.Remove(ev);

        public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync() => await _db.Database.BeginTransactionAsync();

        private IQueryable<Event> WithDetails() =>
            _db.Events.Include(e => e.Venue).Include(e => e.Category)
                .Include(e => e.Seats).Include(e => e.ParkingSlots);

        public async Task<List<Event>> SearchAsync(string? name, DateOnly? date, int? venueId, int? categoryId)
        {
            var query = WithDetails();

            if (!string.IsNullOrWhiteSpace(name)) query = query.Where(e => e.Name.Contains(name));
            if (date.HasValue) query = query.Where(e => e.EventDate == date.Value);
            if (venueId.HasValue) query = query.Where(e => e.VenueId == venueId.Value);
            if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId.Value);

            return await query.OrderBy(e => e.EventDate).ToListAsync();
        }

        public async Task<Event?> GetByIdWithDetailsAsync(int id) =>
            await WithDetails().FirstOrDefaultAsync(e => e.EventId == id);

        // Business Rule: a venue cannot be deleted while it has upcoming events scheduled at it.
        public async Task<bool> HasUpcomingEventsForVenueAsync(int venueId) =>
            await _db.Events.AnyAsync(e => e.VenueId == venueId && e.EventDate >= DateOnly.FromDateTime(DateTime.UtcNow));

        public async Task<int> CountAsync() => await _db.Events.CountAsync();
    }
}
