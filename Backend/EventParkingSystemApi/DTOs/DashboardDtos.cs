namespace EventParkingSystemApi.DTOs
{
    public record CustomerDashboardResponse(
        List<BookingResponse> UpcomingBookings,
        BookingParkingResponse? ReservedParking,
        List<PaymentResponse> RecentPayments,
        int UnreadNotificationCount);

    public record AdminDashboardResponse(
        int TotalEvents, int TotalBookings, int AvailableSeatsSystemWide,
        int OccupiedParkingSlots, decimal TotalRevenue, int TotalCustomers);

}
