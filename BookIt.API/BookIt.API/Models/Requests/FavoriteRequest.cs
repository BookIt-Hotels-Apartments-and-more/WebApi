namespace BookIt.API.Models.Requests;

public record FavoriteRequest
{
    public int UserId { get; init; }
    public int ApartmentId { get; init; }
}
