using BookIt.API.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation;

public class ApartmentRatingValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not ReviewRequest request)
            return ValidationResult.Success;

        if (request.ApartmentId.HasValue)
        {
            var propertyName = validationContext.MemberName ?? "Rating";

            if (value is null)
                return new ValidationResult($"{propertyName} is required when reviewing an apartment");
        }

        return ValidationResult.Success;
    }
}

public class UserRatingValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not ReviewRequest request)
            return ValidationResult.Success;

        if (request.CustomerId.HasValue)
        {
            if (value is null)
                return new ValidationResult("Customer stay rating is required when reviewing a user");
        }

        return ValidationResult.Success;
    }
}