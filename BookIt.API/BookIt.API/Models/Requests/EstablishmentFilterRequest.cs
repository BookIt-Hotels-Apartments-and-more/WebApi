using BookIt.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public class EstablishmentFilterRequest
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
    
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
}
