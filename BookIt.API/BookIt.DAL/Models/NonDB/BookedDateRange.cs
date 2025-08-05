namespace BookIt.DAL.Models.NonDB;

public record BookedDateRange
{
    public int BookingId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public bool IsCheckedIn { get; set; }
}
