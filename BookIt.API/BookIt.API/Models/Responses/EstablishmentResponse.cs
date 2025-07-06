namespace BookIt.API.Models.Responses;

using BookIt.DAL.Models;
using BookIt.BLL.DTOs;

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

public record EstablishmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EstablishmentType Type { get; set; }
    public EstablishmentFeaturesResponse Features { get; set; } = new(); // зміна тут
    public DateTime CreatedAt { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
    public float? Rating { get; set; }
    public OwnerResponse Owner { get; set; } = null!;
    public List<ImageDTO> Photos { get; set; } = new();
}