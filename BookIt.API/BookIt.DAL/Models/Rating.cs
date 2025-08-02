namespace BookIt.DAL.Models;

public class Rating
{
    public int Id { get; set; }

    public float StaffRating { get; set; }
    public float PurityRating { get; set; }
    public float PriceQualityRating { get; set; }
    public float ComfortRating { get; set; }
    public float FacilitiesRating { get; set; }
    public float LocationRating { get; set; }

    public float GeneralRating { get; private set; }

    public int ReviewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedAt { get; set; }

    public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
    public ICollection<Establishment> Establishments { get; set; } = new List<Establishment>();
    public ICollection<User> Users { get; set; } = new List<User>();

    public void UpdateGeneralRating()
    {
        var ratings = new[] { StaffRating, PurityRating, PriceQualityRating, ComfortRating, FacilitiesRating, LocationRating };
        GeneralRating = ratings.Average();
        LastUpdatedAt = DateTime.UtcNow;
    }
}