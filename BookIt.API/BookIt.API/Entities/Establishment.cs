namespace BookIt.Entities
{
    public class Establishment
    {
        public int Id { get; set; }
        
        public string? Name { get; set; }
        public string? Address { get; set; }
        public double Raiting { get; set; }
        public Image[]? Photo { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
    }
}