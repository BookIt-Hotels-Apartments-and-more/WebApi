namespace BookIt.Entities;

public class Image
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string BlobUrl { get; set; } = null!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public User? User { get; set; } = null!;
    public int? EstablishmentId { get; set; }
    public Establishment? Establishment { get; set; }
    public int? ApartmentId { get; set; }
    public Apartment? Apartment { get; set; }
    public int? ReviewId { get; set; }
    public Review? Review { get; set; }
}