namespace BookIt.API.Models.Responses;

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = null!;
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
