namespace BookIt.BLL.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(string base64Image, string containerName, string fileName);
    Task<bool> DeleteImageAsync(string blobUrl, string containerName);
}
