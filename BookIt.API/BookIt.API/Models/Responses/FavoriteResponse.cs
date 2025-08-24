namespace BookIt.API.Models.Responses;

public record FavoriteResponse
{
    public int Id { get; set; }
    public CustomerResponse User { get; set; } = null!;
    public EstablishmentResponse Establishment { get; set; } = null!;
}
