namespace BookIt.BLL.Helpers;

public static class CacheKeys
{
    public const string ReviewsPrefix = "reviews:";

    public static string ReviewsByUserId(int id) => $"{ReviewsPrefix}us:{id}";
    public static string ReviewsByApartmentId(int id) => $"{ReviewsPrefix}ap:{id}";
    public static string ReviewsByEstablishmentId(int id) => $"{ReviewsPrefix}es:{id}";
}
