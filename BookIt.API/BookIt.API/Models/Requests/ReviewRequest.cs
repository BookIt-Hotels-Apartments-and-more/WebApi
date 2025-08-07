using BookIt.API.Models.Requests.Common;
using BookIt.API.Validation;
using BookIt.DAL.Constants;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record ReviewRequest : IHasPhotos
{
    [Required(ErrorMessage = "Review text is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Review text must be between 10 and 2000 characters")]
    public string Text { get; set; } = null!;

    [ApartmentRatingValidation]
    [Range(RatingConstants.MinRating, RatingConstants.MaxRating, ErrorMessage = "Staff rating must be between 1.0 and 10.0")]
    public float? StaffRating { get; set; }

    [ApartmentRatingValidation]
    [Range(RatingConstants.MinRating, RatingConstants.MaxRating, ErrorMessage = "Purity rating must be between 1.0 and 10.0")]
    public float? PurityRating { get; set; }

    [ApartmentRatingValidation]
    [Range(RatingConstants.MinRating, RatingConstants.MaxRating, ErrorMessage = "Price/Quality rating must be between 1.0 and 10.0")]
    public float? PriceQualityRating { get; set; }

    [ApartmentRatingValidation]
    [Range(RatingConstants.MinRating, RatingConstants.MaxRating, ErrorMessage = "Comfort rating must be between 1.0 and 10.0")]
    public float? ComfortRating { get; set; }

    [ApartmentRatingValidation]
    [Range(RatingConstants.MinRating, RatingConstants.MaxRating, ErrorMessage = "Facilities rating must be between 1.0 and 10.0")]
    public float? FacilitiesRating { get; set; }

    [ApartmentRatingValidation]
    [Range(RatingConstants.MinRating, RatingConstants.MaxRating, ErrorMessage = "Location rating must be between 1.0 and 10.0")]
    public float? LocationRating { get; set; }

    [UserRatingValidation]
    [Range(RatingConstants.MinRating, RatingConstants.MaxRating, ErrorMessage = "Customer stay rating must be between 1.0 and 10.0")]
    public float? CustomerStayRating { get; set; }

    [Required(ErrorMessage = "Booking ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Booking ID must be a positive number")]
    public int BookingId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Customer ID must be a positive number")]
    public int? CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Apartment ID must be a positive number")]
    public int? ApartmentId { get; set; }

    [PhotoIdsValidation]
    public List<int> ExistingPhotosIds { get; set; } = new();

    [Base64ImageValidation]
    public List<string> NewPhotosBase64 { get; set; } = new();

    [ReviewTargetValidation]
    public bool IsValid => CustomerId.HasValue ^ ApartmentId.HasValue;

    [PhotoLimitValidation(PhotosNumberConstants.MaxPhotosForReview, isRequired: false)]
    public int TotalPhotosCount => ExistingPhotosIds.Count + NewPhotosBase64.Count;
}