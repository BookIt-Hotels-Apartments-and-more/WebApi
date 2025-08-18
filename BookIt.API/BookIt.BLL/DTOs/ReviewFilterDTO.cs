namespace BookIt.BLL.DTOs;

public record ReviewFilterDTO : PaginationFilterDTO
{
    public int? EstablishmentId { get; set; } = null;
    public int? ApartmentId { get; set; } = null;
    public int? TenantId { get; set; } = null;
}
