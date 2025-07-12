using BookIt.API.Models.Requests.Common;
using BookIt.API.Validation;
using BookIt.DAL.Constants;
using BookIt.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record ApartmentRequest : IHasPhotos
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Name must be between 10 and 100 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public double Price { get; set; }

    [Required(ErrorMessage = "Capacity is required")]
    [Range(1, 50, ErrorMessage = "Capacity must be between 1 and 50")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Area is required")]
    [Range(1.0, 10000.0, ErrorMessage = "Area must be between 1 and 10,000 square meters")]
    public double Area { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 50, ErrorMessage = "Description must be between 50 and 2000 characters")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Features are required")]
    public ApartmentFeatures Features { get; set; }

    [Required(ErrorMessage = "Establishment ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Establishment ID must be a positive number")]
    public int EstablishmentId { get; set; }

    [PhotoIdsValidation]
    public List<int> ExistingPhotosIds { get; set; } = new();

    [Base64ImageValidation]
    public List<string> NewPhotosBase64 { get; set; } = new();

    [PhotoLimitValidation(PhotosNumberConstants.MaxPhotosForApartment, isRequired: true)]
    public int TotalPhotosCount => ExistingPhotosIds.Count + NewPhotosBase64.Count;
}
