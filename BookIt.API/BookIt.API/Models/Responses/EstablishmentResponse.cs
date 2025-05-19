namespace BookIt.BLL.Models.Responses;

public record EstablishmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public OwnerResponse Owner { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}

public record OwnerResponse
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; } = null!;
    public string? Bio { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}
