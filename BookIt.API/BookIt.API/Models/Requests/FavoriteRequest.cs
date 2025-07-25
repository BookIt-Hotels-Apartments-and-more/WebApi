﻿using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record FavoriteRequest
{
    [Required(ErrorMessage = "User ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
    public int UserId { get; init; }

    [Required(ErrorMessage = "Apartment ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Apartment ID must be a positive number")]
    public int ApartmentId { get; init; }
}
