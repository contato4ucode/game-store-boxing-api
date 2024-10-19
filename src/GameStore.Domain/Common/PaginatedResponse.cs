namespace GameStore.Domain.Common;

public class PaginatedResponse<TEntity>
{
    public List<TEntity> Items { get; set; } = new();
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public PaginatedResponse() { }

    public PaginatedResponse(List<TEntity> items, int count, int? pageNumber, int? pageSize)
    {
        Items = items;
        TotalItems = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = pageSize.HasValue && pageSize.Value > 0
            ? (int)Math.Ceiling(count / (double)pageSize.Value)
            : 0;
    }
}
