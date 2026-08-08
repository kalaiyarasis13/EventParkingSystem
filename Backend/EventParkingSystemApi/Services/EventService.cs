using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IParkingSlotRepository _parkingSlotRepository;
        private readonly IBookingRepository _bookingRepository;

        public EventService(
            IEventRepository eventRepository, IVenueRepository venueRepository, ICategoryRepository categoryRepository,
             ISeatRepository seatRepository, IParkingSlotRepository parkingSlotRepository, IBookingRepository bookingRepository)
        {
            _eventRepository = eventRepository;
            _venueRepository = venueRepository;
            _categoryRepository = categoryRepository;
            _seatRepository = seatRepository;
            _parkingSlotRepository = parkingSlotRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<List<EventResponse>> GetAllAsync(string? name, DateOnly? date, int? venueId, int? categoryId)
        {
            var events = await _eventRepository.SearchAsync(name, date, venueId, categoryId);
            return events.Select(MapToResponse).ToList();
        }

        public async Task<EventResponse> GetByIdAsync(int id)
        {
            var ev = await _eventRepository.GetByIdWithDetailsAsync(id) ?? throw ApiException.NotFound("Event not found.");
            return MapToResponse(ev);
        }

        public async Task<EventResponse> CreateAsync(EventCreateRequest request)
        {
            // Business Rule: an event must belong to exactly one venue and one category — validate both exist.
            if (!await _venueRepository.ExistsAsync(request.VenueId))
                throw ApiException.BadRequest("The specified venue does not exist.");

            if (!await _categoryRepository.ExistsAsync(request.CategoryId))
                throw ApiException.BadRequest("The specified category does not exist.");

            using var transaction = await _eventRepository.BeginTransactionAsync();
            try
            {
                var ev = new Event
                {
                    Name = request.Name,
                    VenueId = request.VenueId,
                    CategoryId = request.CategoryId,
                    EventDate = request.EventDate,
                    EventTime = request.EventTime,
                    TicketPrice = request.TicketPrice,
                    ParkingFee = request.ParkingFee,
                    Description = request.Description
                };
                await _eventRepository.AddAsync(ev);
                await _eventRepository.SaveChangesAsync();

                // Generate the seat map: rows A, B, C... x N seats per row
                for (int r = 0; r < request.SeatRows; r++)
                {
                    var rowLetter = ((char)('A' + r)).ToString();
                    for (int n = 1; n <= request.SeatsPerRow; n++)
                    {
                        await _seatRepository.AddAsync(new Seat { EventId = ev.EventId, SeatRow = rowLetter, SeatNumber = n, Status = SeatStatus.Available });
                    }
                }

                // Generate parking slots P1..PN
                for (int p = 1; p <= request.ParkingSlotCount; p++)
                {
                    await _parkingSlotRepository.AddAsync(new ParkingSlot { EventId = ev.EventId, SlotLabel = $"P{p}", Status = ParkingSlotStatus.Available });
                }

                // All three repositories share the same scoped DbContext, so this one SaveChanges
                // call persists the event, its seats, and its parking slots together.
                await _seatRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(ev.EventId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EventResponse> UpdateAsync(int id, EventUpdateRequest request)
        {
            var ev = await _eventRepository.GetByIdAsync(id) ?? throw ApiException.NotFound("Event not found.");

            // Business Rule: ticket price and seat map should not change once bookings exist.
            var hasBookings = await _bookingRepository.HasActiveBookingsForEventAsync(id);
            if (hasBookings && ev.TicketPrice != request.TicketPrice)
                throw ApiException.Conflict("Ticket price cannot be changed after bookings exist for this event.");

            if (!await _venueRepository.ExistsAsync(request.VenueId))
                throw ApiException.BadRequest("The specified venue does not exist.");
            if (!await _categoryRepository.ExistsAsync(request.CategoryId))
                throw ApiException.BadRequest("The specified category does not exist.");

            ev.Name = request.Name;
            ev.VenueId = request.VenueId;
            ev.CategoryId = request.CategoryId;
            ev.EventDate = request.EventDate;
            ev.EventTime = request.EventTime;
            ev.TicketPrice = request.TicketPrice;
            ev.ParkingFee = request.ParkingFee;
            ev.Description = request.Description;
            ev.UpdatedAt = DateTime.UtcNow;
            await _eventRepository.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task DeleteAsync(int id)
        {
            var ev = await _eventRepository.GetByIdAsync(id) ?? throw ApiException.NotFound("Event not found.");

            // Business Rule: an event can only be deleted if it has no active bookings.
            if (await _bookingRepository.HasActiveBookingsForEventAsync(id))
                throw ApiException.Conflict("Cannot delete an event that has active bookings.");

            _eventRepository.Remove(ev);
            await _eventRepository.SaveChangesAsync();
        }

        private static EventResponse MapToResponse(Event e) => new(
            e.EventId, e.Name, e.VenueId, e.Venue?.Name ?? "", e.CategoryId, e.Category?.Name ?? "",
            e.EventDate, e.EventTime, e.TicketPrice, e.ParkingFee, e.Description, e.IsLocked,
            e.Seats.Count, e.Seats.Count(s => s.Status == SeatStatus.Available),
            e.ParkingSlots.Count, e.ParkingSlots.Count(p => p.Status == ParkingSlotStatus.Available));
    }
}
