namespace EventParkingSystemApi.DTOs
{
    public record CategoryResponse(int CategoryId, string Name);
    public record CategoryCreateRequest(string Name);
}
