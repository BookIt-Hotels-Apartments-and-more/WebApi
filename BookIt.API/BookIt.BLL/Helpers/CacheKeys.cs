using BookIt.BLL.DTOs;

namespace BookIt.BLL.Helpers;

public static class CacheKeys
{
    public const string ReviewsPrefix = "reviews:";

    public static string ReviewById(int id) => $"{ReviewsPrefix}id:{id}";

    public static string ReviewsByFilter(ReviewFilterDTO filter)
    {
        var keyParts = new List<string> { $"{ReviewsPrefix}filtered" };

        if (filter.TenantId.HasValue) keyParts.Add($"tenant:{filter.TenantId}");
        if (filter.ApartmentId.HasValue) keyParts.Add($"apartment:{filter.ApartmentId}");
        if (filter.EstablishmentId.HasValue) keyParts.Add($"establishment:{filter.EstablishmentId}");

        keyParts.Add($"page:{filter.Page}");
        keyParts.Add($"size:{filter.PageSize}");

        return string.Join(":", keyParts);
    }
}