using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation.Attributes;

public class Base64ImageValidationAttribute : ValidationAttribute
{
    private static readonly string[] AllowedImageTypes = { "jpeg", "jpg", "png", "webp" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> base64Images)
            return ValidationResult.Success;

        var errors = new List<string>();

        for (int i = 0; i < base64Images.Count; i++)
        {
            var base64 = base64Images[i];

            if (string.IsNullOrWhiteSpace(base64))
            {
                errors.Add($"Photo {i + 1}: Base64 string cannot be empty");
                continue;
            }

            try
            {
                if (!base64.StartsWith("data:image/"))
                {
                    errors.Add($"Photo {i + 1}: Invalid base64 image format");
                    continue;
                }

                var mimeType = base64.Split(';')[0].Split(':')[1];
                var imageType = mimeType.Split('/')[1];

                if (!AllowedImageTypes.Contains(imageType.ToLower()))
                {
                    errors.Add($"Photo {i + 1}: Image type '{imageType}' is not allowed. Allowed types: {string.Join(", ", AllowedImageTypes)}");
                    continue;
                }

                var base64Data = base64.Split(',')[1];
                var imageBytes = Convert.FromBase64String(base64Data);

                if (imageBytes.Length > MaxFileSizeBytes)
                {
                    errors.Add($"Photo {i + 1}: Image size exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)}MB");
                }
            }
            catch (Exception)
            {
                errors.Add($"Photo {i + 1}: Invalid base64 format");
            }
        }

        return errors.Count > 0
            ? new ValidationResult(string.Join("; ", errors))
            : ValidationResult.Success;
    }
}
