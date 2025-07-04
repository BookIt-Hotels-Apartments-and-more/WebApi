namespace BookIt.API.Models.Requests;

public record UserImagesRequest
{
    public List<int> ExistingPhotosIds { get; set; } = new();
    public List<string> NewPhotosBase64 { get; set; } = new();
}
