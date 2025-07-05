namespace BookIt.API.Models.Requests;

using BookIt.DAL.Models;

public record EstablishmentRequest
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EstablishmentType Type { get; set; }
    public EstablishmentFeatures Features { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
    public int OwnerId { get; set; }
    public List<int> ExistingPhotosIds { get; set; } = new();
    public List<string> NewPhotosBase64 { get; set; } = new();
}