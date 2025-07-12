using BookIt.API.Models.Requests.Common;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation;

public class PhotoLimitValidationAttribute : ValidationAttribute
{
    private readonly int _maxTotalPhotos;
    private readonly bool _isRequired;

    public PhotoLimitValidationAttribute(int maxTotalPhotos, bool isRequired = true)
    {
        _maxTotalPhotos = maxTotalPhotos;
        _isRequired = isRequired;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not IHasPhotos request)
            return ValidationResult.Success;

        var totalPhotos = request.ExistingPhotosIds.Count + request.NewPhotosBase64.Count;

        if (_isRequired && totalPhotos == 0)
            return new ValidationResult("At least one photo is required");

        if (totalPhotos > _maxTotalPhotos)
            return new ValidationResult($"Total number of photos cannot exceed {_maxTotalPhotos}. Current total: {totalPhotos} (existing: {request.ExistingPhotosIds.Count}, new: {request.NewPhotosBase64.Count})");

        return ValidationResult.Success;
    }
}