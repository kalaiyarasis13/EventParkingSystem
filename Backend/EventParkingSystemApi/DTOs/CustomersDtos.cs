namespace EventParkingSystemApi.DTOs;
public record CustomerResponse(int CustomerId, string FullName, string Email, string? Phone, string Role, bool IsActive, DateTime CreatedAt);
public record CustomerUpdateRequest(string FullName, string? Phone);
public record CustomerProfileResponse(CustomerResponse Customer, int TotalBookings, int UpcomingBookings);
