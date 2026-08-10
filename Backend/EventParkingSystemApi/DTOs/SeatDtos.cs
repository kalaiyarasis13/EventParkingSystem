namespace EventParkingSystemApi.DTOs;

public record CreateSeatRequest(int EventId,string SeatRow,int SeatNumber
);

public record UpdateSeatRequest(string SeatRow,int SeatNumber
);

public record SeatResponse( int SeatId, int EventId, string SeatRow,
    int SeatNumber, string Status
);