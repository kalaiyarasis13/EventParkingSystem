namespace EventParkingSystemApi.DTOs;

public record CreateHoldRequest(int SeatId,int EventId
);

public record HoldResponse(int HoldId,int SeatId,int EventId,
    int CustomerId,DateTime HeldAt, DateTime ExpiresAt,string Status
);