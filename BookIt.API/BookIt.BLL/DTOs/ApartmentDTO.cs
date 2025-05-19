namespace BookIt.BLL.DTOs;

public record ApartmentDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int Capacity { get; set; }
    public float? Rating { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int EstablishmentId { get; set; }
    public EstablishmentDTO Establishment { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}
