namespace EventParkingSystemApi.DTOs;

public record CreateParkingSlotRequest(int EventId,string SlotLabel
);

public record UpdateParkingSlotRequest(string SlotLabel
);

public record ParkingSlotResponse( int ParkingSlotId,int EventId,string SlotLabel,string Status
);