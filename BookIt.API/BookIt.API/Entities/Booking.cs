namespace BookIt.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool IsPayed { get; set; }
        public bool IsCheckedIn { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; } = null!;
    }
}
