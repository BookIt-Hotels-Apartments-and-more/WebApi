using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record ReviewFilterRequest : PaginationRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Establishment ID must be a positive number")]
    public int? EstablishmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Apartment ID must be a positive number")]
    public int? ApartmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Tenant ID must be a positive number")]
    public int? TenantId { get; set; }
}
