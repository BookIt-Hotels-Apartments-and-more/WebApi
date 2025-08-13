using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record TrendingEstablishmentsRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Count must be greater than 0")]
    public int Count { get; init; } = 10;

    [Range(1, int.MaxValue, ErrorMessage = "Trending period must be either omitted or greater than 0")]
    public int? PastDays { get; init; } = null;
}
