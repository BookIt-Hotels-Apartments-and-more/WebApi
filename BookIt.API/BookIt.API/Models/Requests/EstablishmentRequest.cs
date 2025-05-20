namespace BookIt.API.Models.Requests;

public record EstablishmentRequest
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OwnerId { get; set; }
    public List<string> Photos { get; set; } = new();
}