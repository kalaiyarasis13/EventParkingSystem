using EventParkingSystemApi.Data;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Repositories;

public class ParkingReservationRepository : IParkingReservationRepository
{
    private readonly AppDbContext _db;

    public ParkingReservationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ParkingReservation?> GetByIdAsync(
        int parkingReservationId)
    {
        return await _db.ParkingReservations
            .Include(pr => pr.Booking)
            .Include(pr => pr.ParkingSlot)
            .FirstOrDefaultAsync(
                pr => pr.ParkingReservationId == parkingReservationId);
    }

    public async Task<ParkingReservation?> GetByBookingIdAsync(
        int bookingId)
    {
        return await _db.ParkingReservations
            .Include(pr => pr.Booking)
            .Include(pr => pr.ParkingSlot)
            .FirstOrDefaultAsync(
                pr => pr.BookingId == bookingId);
    }

    public async Task<ParkingReservation?> GetByParkingSlotIdAsync(
        int parkingSlotId)
    {
        return await _db.ParkingReservations
            .Include(pr => pr.Booking)
            .Include(pr => pr.ParkingSlot)
            .FirstOrDefaultAsync(
                pr => pr.ParkingSlotId == parkingSlotId);
    }

    public async Task<List<ParkingReservation>> GetByCustomerAsync(
        int customerId)
    {
        return await _db.ParkingReservations
            .Include(pr => pr.Booking)
            .Include(pr => pr.ParkingSlot)
            .Where(pr => pr.Booking!.CustomerId == customerId)
            .OrderByDescending(pr => pr.ReservedAt)
            .ToListAsync();
    }

    public async Task AddAsync(ParkingReservation reservation)
    {
        await _db.ParkingReservations.AddAsync(reservation);
    }

    public void Update(ParkingReservation reservation)
    {
        _db.ParkingReservations.Update(reservation);
    }

    public void Remove(ParkingReservation reservation)
    {
        _db.ParkingReservations.Remove(reservation);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}