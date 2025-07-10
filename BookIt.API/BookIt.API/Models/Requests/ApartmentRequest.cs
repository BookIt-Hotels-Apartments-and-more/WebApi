namespace BookIt.API.Models.Requests;

using BookIt.DAL.Models;

public record ApartmentRequest
{
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int Capacity { get; set; }
    public double Area { get; set; }
    public string Description { get; set; } = null!;
    public ApartmentFeatures Features { get; set; }
    public int EstablishmentId { get; set; }
    public List<int> ExistingPhotosIds { get; set; } = new();
    public List<string> NewPhotosBase64 { get; set; } = new();
}