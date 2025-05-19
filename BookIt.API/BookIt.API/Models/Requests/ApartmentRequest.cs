namespace BookIt.API.Models.Requests;

public record ApartmentRequest
{
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int Capacity { get; set; }
    public string Description { get; set; } = null!;
    public int EstablishmentId { get; set; }
}
