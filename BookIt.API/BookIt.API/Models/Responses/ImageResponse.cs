namespace BookIt.API.Models.Responses;

public record ImageResponse
{
    public int Id { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
}
