using BookIt.DAL.Enums;

namespace BookIt.BLL.DTOs;

public class EstablishmentFilterDTO
{
    public string? Name { get; set; }
    public EstablishmentType? Type { get; set; }
    public EstablishmentFeatures? Features { get; set; }
    public int? OwnerId { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public float? MinRating { get; set; }
    public float? MaxRating { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
