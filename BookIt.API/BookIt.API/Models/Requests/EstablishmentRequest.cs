namespace BookIt.API.Models.Requests;

public record EstablishmentRequest
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OwnerId { get; set; }
    public List<int> ExistingPhotosIds { get; set; } = new();
    public List<string> NewPhotosBase64 { get; set; } = new();
}