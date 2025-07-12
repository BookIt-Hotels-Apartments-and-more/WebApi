using BookIt.API.Models.Requests.Common;
using BookIt.API.Validation;

namespace BookIt.API.Models.Requests;

public record UserImagesRequest : IHasPhotos
{
    [PhotoIdsValidation]
    public List<int> ExistingPhotosIds { get; set; } = new();

    [Base64ImageValidation]
    public List<string> NewPhotosBase64 { get; set; } = new();

    [PhotoLimitValidation(5, isRequired: false)]
    public int TotalPhotosCount => ExistingPhotosIds.Count + NewPhotosBase64.Count;
}
