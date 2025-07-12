using BookIt.API.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation;

public class CheckOutTimeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not EstablishmentRequest request)
            return ValidationResult.Success;

        var checkInTime = request.CheckInTime;
        var checkOutTime = request.CheckOutTime;

        if (checkInTime <= checkOutTime)
            return new ValidationResult("Check-in time must be after check-out time to allow time for room preparation");

        if (checkOutTime.Hour < 6 || checkOutTime.Hour > 12)
            return new ValidationResult("Check-out time should be between 06:00 and 12:00 (morning hours)");

        if (checkInTime.Hour < 12 || checkInTime.Hour > 23)
            return new ValidationResult("Check-in time should be between 12:00 and 23:00 (afternoon/evening hours)");

        // 1+ hour gap for cleaning
        var timeDifference = checkInTime.Hour - checkOutTime.Hour;
        if (timeDifference < 1)
            return new ValidationResult("There must be at least 1 hour between check-out and check-in times for room preparation");

        return ValidationResult.Success;
    }
}
