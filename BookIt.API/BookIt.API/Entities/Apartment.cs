namespace BookIt.Entities
{
    public class Apartment
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Price { get; set; }
        public Image[]? Photo { get; set; }

        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; } = null!;
    }
}