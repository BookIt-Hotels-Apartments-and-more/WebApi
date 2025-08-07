namespace BookIt.DAL.Models;

public class UserRating
{
    public int Id { get; set; }
    public float CustomerStayRating { get; set; }
    public int ReviewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}