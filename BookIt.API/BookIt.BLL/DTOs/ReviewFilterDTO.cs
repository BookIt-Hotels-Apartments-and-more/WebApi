namespace BookIt.BLL.DTOs;

public record ReviewFilterDTO
{
    public int? EstablishmentId { get; set; } = null;
    public int? ApartmentId { get; set; } = null;
    public int? TenantId { get; set; } = null;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
