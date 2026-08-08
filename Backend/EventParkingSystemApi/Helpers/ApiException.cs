namespace EventParkingSystemApi.Helpers;

// Thrown by services; translated to the correct HTTP status code by controllers/middleware.
public class ApiException : Exception
{
    public int StatusCode { get; }
    public ApiException(int statusCode, string message) : base(message) => StatusCode = statusCode;

    public static ApiException BadRequest(string msg) => new(400, msg);
    public static ApiException Unauthorized(string msg) => new(401, msg);
    public static ApiException Forbidden(string msg) => new(403, msg);
    public static ApiException NotFound(string msg) => new(404, msg);
    public static ApiException Conflict(string msg) => new(409, msg);
}
