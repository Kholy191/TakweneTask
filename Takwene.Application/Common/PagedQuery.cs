namespace Takwene.Application.Common;

// Paging input shared by list endpoints.
public class PagedQuery
{
    public const int MaxPageSize = 100;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
