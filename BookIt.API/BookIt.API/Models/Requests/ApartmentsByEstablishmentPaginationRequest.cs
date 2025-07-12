using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public class ApartmentsByEstablishmentPaginationRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
}
