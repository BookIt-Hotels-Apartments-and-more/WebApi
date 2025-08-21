using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation.Attributes;

public class BookingDateValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime dateFrom)
            return ValidationResult.Success;

        var today = DateTime.Today;
        var maxAdvanceBooking = today.AddYears(2);

        if (dateFrom.Date < today)
            return new ValidationResult("Booking start date cannot be in the past");

        if (dateFrom.Date > maxAdvanceBooking)
            return new ValidationResult("Booking start date cannot be more than 2 years in advance");

        return ValidationResult.Success;
    }
}