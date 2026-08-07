using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepository;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.IRepositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly AppDbContext _db;

    public FeedbackRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Feedback?> GetByIdAsync(int feedbackId)
    {
        return await _db.Feedbacks
            .Include(f => f.Booking)
            .Include(f => f.Customer)
            .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
    }

    public async Task<Feedback?> GetByBookingIdAsync(int bookingId)
    {
        return await _db.Feedbacks
            .Include(f => f.Booking)
            .Include(f => f.Customer)
            .FirstOrDefaultAsync(f => f.BookingId == bookingId);
    }

    public async Task<List<Feedback>> GetByCustomerIdAsync(int customerId)
    {
        return await _db.Feedbacks
            .Include(f => f.Booking)
            .Include(f => f.Customer)
            .Where(f => f.CustomerId == customerId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Feedback feedback)
    {
        await _db.Feedbacks.AddAsync(feedback);
    }

    public void Update(Feedback feedback)
    {
        _db.Feedbacks.Update(feedback);
    }

    public void Remove(Feedback feedback)
    {
        _db.Feedbacks.Remove(feedback);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}