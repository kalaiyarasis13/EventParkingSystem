namespace EventParkingSystemApi.DTOs;

public record CreateParkingReservationRequest(int BookingId,int ParkingSlotId
);

public record ParkingReservationResponse( int ParkingReservationId,int BookingId,int ParkingSlotId,
    string SlotLabel,decimal FeeAtBooking, DateTime ReservedAt
);