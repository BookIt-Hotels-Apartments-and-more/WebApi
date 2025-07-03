namespace BookIt.BLL.DTOs;

public record ImageDTO
{
    public int? Id { get; set; }
    public string? BlobUrl { get; set; }
    public string? Base64Image { get; set; }
}
