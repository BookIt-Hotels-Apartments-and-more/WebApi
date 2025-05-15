namespace BookIt.Entities
{
    public enum UserRole {
        Admin, // администратор
        HotelOwner, // владелец отеля
        Consumer, // потребитель
    }

    public class User {
        public int Id { get; set; }

        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public double Rating { get; set; }

        public UserRole Role { get; set; }
        public Image? Photo { get; set; }
    }
}