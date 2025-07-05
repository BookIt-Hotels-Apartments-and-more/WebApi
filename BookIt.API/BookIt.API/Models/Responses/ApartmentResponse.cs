namespace BookIt.API.Models.Responses;

public record ApartmentFeaturesResponse
{
    public bool FreeWifi { get; set; }
    public bool AirConditioning { get; set; }
    public bool Breakfast { get; set; }
    public bool Kitchen { get; set; }
    public bool TV { get; set; }
    public bool Balcony { get; set; }
    public bool Bathroom { get; set; }
    public bool PetsAllowed { get; set; }
}

public record ApartmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int Capacity { get; set; }
    public double Area { get; set; }
    public float? Rating { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public EstablishmentResponse Establishment { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
    public ApartmentFeaturesResponse Features { get; set; } = new();
}