namespace BookIt.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public string? Text { get; set; }
        public double Rating { get; set; }
        public Image[]? Photo { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; } = null!;
    }
}