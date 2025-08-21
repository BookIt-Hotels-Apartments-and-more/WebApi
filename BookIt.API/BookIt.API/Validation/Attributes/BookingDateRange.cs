using BookIt.API.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation.Attributes;

public class BookingDateRangeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not BookingRequest request)
            return ValidationResult.Success;

        var dateFrom = request.DateFrom;
        var dateTo = request.DateTo;

        if (dateTo <= dateFrom)
            return new ValidationResult("End date must be after start date");

        var stayDuration = dateTo.Date - dateFrom.Date;
        if (stayDuration.TotalDays < 1)
            return new ValidationResult("Minimum stay is 1 night");

        if (stayDuration.TotalDays > 30)
            return new ValidationResult("Maximum stay is 30 days");

        var today = DateTime.Today;
        var maxAdvanceBooking = today.AddYears(2);

        if (dateTo.Date > maxAdvanceBooking)
            return new ValidationResult("Booking end date cannot be more than 2 years in advance");

        var daysInAdvance = (dateFrom.Date - today).TotalDays;
        if (daysInAdvance > 365)
            return new ValidationResult("Bookings cannot be made more than 365 days in advance");

        return ValidationResult.Success;
    }
}
