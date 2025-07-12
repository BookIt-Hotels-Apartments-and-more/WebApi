using BookIt.API.Models.Requests.Common;
using BookIt.API.Validation;
using BookIt.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record EstablishmentRequest : IHasPhotos
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Name must be between 10 and 100 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 50, ErrorMessage = "Description must be between 50 and 2000 characters")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Type is required")]
    [EnumDataType(typeof(EstablishmentType), ErrorMessage = "Invalid establishment type")]
    public EstablishmentType Type { get; set; }

    [Required(ErrorMessage = "Features are required")]
    public EstablishmentFeatures Features { get; set; }

    [Required(ErrorMessage = "Check-in time is required")]
    public TimeOnly CheckInTime { get; set; }

    [Required(ErrorMessage = "Check-out time is required")]
    [CheckOutTimeValidation]
    public TimeOnly CheckOutTime { get; set; }

    [Required(ErrorMessage = "Owner ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Owner ID must be a positive number")]
    public int OwnerId { get; set; }

    [PhotoIdsValidation]
    public List<int> ExistingPhotosIds { get; set; } = new();

    [Base64ImageValidation]
    public List<string> NewPhotosBase64 { get; set; } = new();

    [Required(ErrorMessage = "Latitude is required")]
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required")]
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
    public double Longitude { get; set; }

    [PhotoLimitValidation(20, isRequired: true)]
    public int TotalPhotosCount => ExistingPhotosIds.Count + NewPhotosBase64.Count;
}
