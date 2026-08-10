namespace EventParkingSystemApi.DTOs
{
    public record RegisterRequest(string FullName, string Email, string Phone, string Password);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, int CustomerId, string FullName, string Email, string Role);

}
