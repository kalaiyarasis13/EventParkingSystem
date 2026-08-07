using System.Security.Claims;

namespace EventParkingSystemApi.Helpers;

public static class ControllerExtensions
{
    public static int GetCustomerId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Role)?.Value == "Admin";
}
