namespace BookIt.API.Models.Requests.Common;

public interface IHasPhotos
{
    List<int> ExistingPhotosIds { get; set; }
    List<string> NewPhotosBase64 { get; set; }
}
