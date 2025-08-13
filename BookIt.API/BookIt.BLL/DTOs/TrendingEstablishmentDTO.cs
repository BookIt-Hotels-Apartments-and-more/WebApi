namespace BookIt.BLL.DTOs;

public record TrendingEstablishmentDTO : EstablishmentDTO
{
    public int BookingsCount { get; set; } = 0;
}
