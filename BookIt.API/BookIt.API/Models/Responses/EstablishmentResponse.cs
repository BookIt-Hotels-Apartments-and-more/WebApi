using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.API.Models.Responses;

public record EstablishmentFeaturesResponse
{
    public bool Parking { get; set; }
    public bool Pool { get; set; }
    public bool Beach { get; set; }
    public bool Fishing { get; set; }
    public bool Sauna { get; set; }
    public bool Restaurant { get; set; }
    public bool Smoking { get; set; }
    public bool AccessibleForDisabled { get; set; }
    public bool ElectricCarCharging { get; set; }
    public bool Elevator { get; set; }
}

public record GeolocationResponse 
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
}

public record EstablishmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EstablishmentType Type { get; set; }
    public EstablishmentFeaturesResponse Features { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
    public float? Rating { get; set; }
    public OwnerResponse Owner { get; set; } = null!;
    public GeolocationResponse Geolocation { get; set; } = null!;
    public List<ImageDTO> Photos { get; set; } = new();
}