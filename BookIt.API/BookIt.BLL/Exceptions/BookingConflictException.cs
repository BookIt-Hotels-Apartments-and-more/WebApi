namespace BookIt.BLL.Exceptions;

public class BookingConflictException : BusinessRuleViolationException
{
    public int ApartmentId { get; }
    public DateTime DateFrom { get; }
    public DateTime DateTo { get; }
    public List<string> ConflictingBookings { get; }

    public BookingConflictException(int apartmentId, DateTime dateFrom, DateTime dateTo, List<string> conflictingBookings)
        : base("BOOKING_CONFLICT", $"Apartment {apartmentId} is not available from {dateFrom:yyyy-MM-dd} to {dateTo:yyyy-MM-dd}")
    {
        ApartmentId = apartmentId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        ConflictingBookings = conflictingBookings;

        Properties["ApartmentId"] = apartmentId;
        Properties["DateFrom"] = dateFrom;
        Properties["DateTo"] = dateTo;
        Properties["ConflictingBookings"] = conflictingBookings;
    }
}