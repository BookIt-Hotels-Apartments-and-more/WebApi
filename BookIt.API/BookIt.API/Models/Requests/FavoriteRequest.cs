using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record FavoriteRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
    public int? UserId { get; set; } = null;

    [Required(ErrorMessage = "Establishment ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Establishment ID must be a positive number")]
    public int EstablishmentId { get; init; }
}
