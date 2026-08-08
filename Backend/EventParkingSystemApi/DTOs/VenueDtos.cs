namespace EventParkingSystemApi.DTOs;

public record CreateVenueRequest( string Name,string Address, int TotalCapacity
);

public record UpdateVenueRequest( string Name,string Address,int TotalCapacity
);

public record VenueResponse(int VenueId,string Name,string Address,
    int TotalCapacity,DateTime CreatedAt,DateTime UpdatedAt
);