namespace BookIt.BLL.DTOs;

public record GeolocationDTO
{
    public int? Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
}
