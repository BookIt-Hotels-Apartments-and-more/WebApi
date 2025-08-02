namespace BookIt.BLL.DTOs;

public record RatingDTO
{
    public int Id { get; set; }

    public float StaffRating { get; set; }
    public float PurityRating { get; set; }
    public float PriceQualityRating { get; set; }
    public float ComfortRating { get; set; }
    public float FacilitiesRating { get; set; }
    public float LocationRating { get; set; }
    public float GeneralRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
}