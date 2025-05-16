namespace BookIt.DAL.Models;

public class Establishment
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<Image> Photos { get; set; } = new List<Image>();
    public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
}