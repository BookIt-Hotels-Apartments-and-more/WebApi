using BookIt.API.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation;

public class ReviewTargetValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not ReviewRequest request)
            return ValidationResult.Success;

        var hasCustomerId = request.CustomerId.HasValue;
        var hasApartmentId = request.ApartmentId.HasValue;

        if (!hasCustomerId && !hasApartmentId)
            return new ValidationResult("Either Customer ID or Apartment ID must be specified");

        if (hasCustomerId && hasApartmentId)
            return new ValidationResult("Only one of Customer ID or Apartment ID can be specified, not both");

        return ValidationResult.Success;
    }
}
