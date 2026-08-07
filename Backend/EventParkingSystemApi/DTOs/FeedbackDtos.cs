namespace EventParkingSystemApi.DTOs;

public record CreateFeedbackRequest(int BookingId,int Rating,string? Comment
);

public record UpdateFeedbackRequest(int Rating,string? Comment
);

public record FeedbackResponse(int FeedbackId, int BookingId, int CustomerId,
    int Rating,string? Comment,DateTime CreatedAt
);