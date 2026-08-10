namespace EventParkingSystemApi.DTOs
{
    public record ApiError(string Message);
    public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
}
