namespace EventParkingSystemApi.Helpers;

public static class BookingNumberGenerator
{
    // Format: BKG-<year>-<6 digit sequence based on booking id>
    public static string Generate(int bookingId, int year) => $"BKG-{year}-{bookingId:D6}";
}
