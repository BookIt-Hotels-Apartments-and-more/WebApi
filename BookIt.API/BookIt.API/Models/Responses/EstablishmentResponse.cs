namespace BookIt.API.Models.Responses;

public record EstablishmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public float? Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public OwnerResponse Owner { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}
