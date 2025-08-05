namespace BookIt.BLL.DTOs;

public record ApartmentAvailabilityDTO
{
    public int ApartmentId { get; set; }
    public List<DateTime> UnavailableDates { get; set; } = new();
    public List<BookedPeriod> BookedPeriods { get; set; } = new();
}

public record BookedPeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int BookingId { get; set; }
}