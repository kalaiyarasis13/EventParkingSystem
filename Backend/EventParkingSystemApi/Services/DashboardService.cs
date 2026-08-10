using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IParkingSlotRepository _parkingSlotRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingService _bookings;
        private readonly IPaymentService _payments;
        private readonly INotificationService _notifications;

        public DashboardService(
            IEventRepository eventRepository, IBookingRepository bookingRepository, ISeatRepository seatRepository,
            IParkingSlotRepository parkingSlotRepository, IPaymentRepository paymentRepository, ICustomerRepository customerRepository,
            IBookingService bookings, IPaymentService payments, INotificationService notifications)
        {
            _eventRepository = eventRepository;
            _bookingRepository = bookingRepository;
            _seatRepository = seatRepository;
            _parkingSlotRepository = parkingSlotRepository;
            _paymentRepository = paymentRepository;
            _customerRepository = customerRepository;
            _bookings = bookings;
            _payments = payments;
            _notifications = notifications;
        }

        public async Task<CustomerDashboardResponse> GetCustomerDashboardAsync(int customerId)
        {
            var allBookings = await _bookings.GetByCustomerAsync(customerId);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var upcoming = allBookings.Where(b => b.Status != BookingStatus.Cancelled && b.EventDate >= today).ToList();

            var reservedParking = upcoming.Select(b => b.Parking).FirstOrDefault(p => p is not null);
            var recentPayments = (await _payments.GetByCustomerAsync(customerId)).Take(5).ToList();
            var unreadCount = await _notifications.GetUnreadCountAsync(customerId);

            return new CustomerDashboardResponse(upcoming, reservedParking, recentPayments, unreadCount);
        }

        public async Task<AdminDashboardResponse> GetAdminDashboardAsync()
        {
            var totalEvents = await _eventRepository.CountAsync();
            var totalBookings = await _bookingRepository.CountActiveAsync();
            var availableSeats = await _seatRepository.CountAvailableAsync();
            var occupiedParking = await _parkingSlotRepository.CountOccupiedAsync();
            var totalRevenue = await _paymentRepository.SumTotalRevenueAsync();
            var totalCustomers = await _customerRepository.CountByRoleAsync("Customer");

            return new AdminDashboardResponse(totalEvents, totalBookings, availableSeats, occupiedParking, totalRevenue, totalCustomers);
        }
    }
}
