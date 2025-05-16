namespace BookIt.Entities;

public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ApartmentId { get; set; }
    public Apartment Apartment { get; set; } = null!;
}