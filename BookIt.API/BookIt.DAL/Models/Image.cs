namespace BookIt.DAL.Models;

public class Image
{
    public int Id { get; set; }
    public string BlobUrl { get; set; } = null!;

    public int? UserId { get; set; }
    public User? User { get; set; } = null!;

    public int? EstablishmentId { get; set; }
    public Establishment? Establishment { get; set; }

    public int? ApartmentId { get; set; }
    public Apartment? Apartment { get; set; }

    public int? ReviewId { get; set; }
    public Review? Review { get; set; }
}