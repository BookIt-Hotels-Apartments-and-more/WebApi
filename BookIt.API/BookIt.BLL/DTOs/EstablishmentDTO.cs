namespace BookIt.BLL.DTOs;

public record EstablishmentDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public float? Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OwnerId { get; set; }
    public OwnerDTO Owner { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}
