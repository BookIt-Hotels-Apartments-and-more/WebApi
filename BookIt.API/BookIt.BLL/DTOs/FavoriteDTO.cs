namespace BookIt.BLL.DTOs;

public record FavoriteDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public CustomerDTO User { get; set; } = null!;
    public int EstablishmentId { get; set; }
    public EstablishmentDTO Establishment { get; set; } = null!;
}
