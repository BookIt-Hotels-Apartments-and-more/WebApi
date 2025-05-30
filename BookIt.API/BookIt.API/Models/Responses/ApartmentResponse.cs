﻿namespace BookIt.API.Models.Responses;

public record ApartmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int Capacity { get; set; }
    public float? Rating { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public EstablishmentResponse Establishment { get; set; } = null!;
    public List<string> Photos { get; set; } = new();
}
