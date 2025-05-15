namespace BookIt.Entities
{
    public class Image
    {
        public int Id { get; set; }

        public byte[] Data { get; set; } = null!;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}