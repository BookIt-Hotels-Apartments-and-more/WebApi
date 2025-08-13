namespace BookIt.API.Models.Responses;

public record TrendingEstablishmentResponse : EstablishmentResponse
{
    public int BookingsCount { get; set; } = 0;
}
