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

    public const string FavoritesPrefix = "favorites:";
    public static string FavoritesByUserId(int userId) => $"{FavoritesPrefix}user:{userId}";
    public static string FavoritesCountByEstablishmentId(int establishmentId) => $"{FavoritesPrefix}count:establishment:{establishmentId}";

    public const string ApartmentsPrefix = "apartments:";
    public static string ApartmentById(int id) => $"{ApartmentsPrefix}id:{id}";
    public static string ApartmentsByEstablishmentId(int establishmentId, int? page = null, int? pageSize = null)
    {
        var keyParts = new List<string>
        {
            $"{ApartmentsPrefix}filtered",
            $"establishment:{establishmentId}",
        };

        if (page.HasValue) keyParts.Add($"page:{page}");
        if (pageSize.HasValue) keyParts.Add($"size:{pageSize}");

        return string.Join(":", keyParts);
    }

    public const string EstablishmentsPrefix = "establishments:";
    public static string EstablishmentById(int id) => $"{EstablishmentsPrefix}id:{id}";
    public static string EstablishmentTrending(int size, int? days) => $"{EstablishmentsPrefix}size:{size}:days:{days ?? -1}";
    public static string EstablishmentsByFilter(EstablishmentFilterDTO filter)
    {
        var keyParts = new List<string> { $"{EstablishmentsPrefix}filtered" };

        if (!string.IsNullOrEmpty(filter.Name)) keyParts.Add($"name:{filter.Name}");
        if (filter.Vibe.HasValue) keyParts.Add($"vibe:{filter.Vibe}");
        if (filter.Type.HasValue) keyParts.Add($"type:{filter.Type}");
        if (filter.Features.HasValue) keyParts.Add($"features:{filter.Features}");
        if (filter.OwnerId.HasValue) keyParts.Add($"owner:{filter.OwnerId}");
        if (!string.IsNullOrEmpty(filter.Country)) keyParts.Add($"country:{filter.Country}");
        if (!string.IsNullOrEmpty(filter.City)) keyParts.Add($"city:{filter.City}");
        if (filter.MinRating.HasValue) keyParts.Add($"minrating:{filter.MinRating}");
        if (filter.MaxRating.HasValue) keyParts.Add($"maxrating:{filter.MaxRating}");
        if (filter.MinPrice.HasValue) keyParts.Add($"minprice:{filter.MinPrice}");
        if (filter.MaxPrice.HasValue) keyParts.Add($"maxprice:{filter.MaxPrice}");
        if (filter.Capacity.HasValue) keyParts.Add($"capacity:{filter.Capacity}");
        if (filter.DateFrom.HasValue) keyParts.Add($"datefrom:{filter.DateFrom}");
        if (filter.DateTo.HasValue) keyParts.Add($"dateto:{filter.DateTo}");

        keyParts.Add($"page:{filter.Page}");
        keyParts.Add($"size:{filter.PageSize}");

        return string.Join(":", keyParts);
    }
}