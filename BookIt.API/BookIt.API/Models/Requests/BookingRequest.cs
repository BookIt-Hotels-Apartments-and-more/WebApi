using BookIt.API.Validation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record BookingRequest
{
    [Required(ErrorMessage = "Date from is required")]
    [BookingDateValidation]
    public DateTime DateFrom { get; set; }

    [Required(ErrorMessage = "Date to is required")]
    [BookingDateRangeValidation]
    public DateTime DateTo { get; set; }

    [Required(ErrorMessage = "Customer ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Customer ID must be a positive number")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Apartment ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Apartment ID must be a positive number")]
    public int ApartmentId { get; set; }

    [StringLength(1000, ErrorMessage = "Additional requests cannot exceed 1000 characters")]
    public string? AdditionalRequests { get; set; }
}
