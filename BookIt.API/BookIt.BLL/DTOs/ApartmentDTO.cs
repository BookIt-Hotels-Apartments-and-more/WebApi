namespace BookIt.BLL.DTOs;

using BookIt.DAL.Models;

public record ApartmentDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public double Area { get; set; }
    public int Capacity { get; set; }
    public float? Rating { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int EstablishmentId { get; set; }
    public ApartmentFeatures Features { get; set; } = new();
    public EstablishmentDTO Establishment { get; set; } = null!;
    public List<ImageDTO> Photos { get; set; } = new();
}
