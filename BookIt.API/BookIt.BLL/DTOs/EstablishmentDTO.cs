namespace BookIt.BLL.DTOs;

using BookIt.DAL.Models;

public record EstablishmentDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public EstablishmentType Type { get; set; }
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public float? Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public EstablishmentFeatures Features { get; set; } = new();
    public int OwnerId { get; set; }
    public OwnerDTO Owner { get; set; } = null!;
    public List<ImageDTO> Photos { get; set; } = new();
}