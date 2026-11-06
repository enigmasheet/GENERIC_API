using System.Diagnostics;
using ModularMonolith.SharedKernel.Pagination;

namespace ModularMonolith.Infrastructure.Api;

public sealed record PagedResponse<T>
{
    public bool Success { get; init; } = true;

    public string? Message { get; init; }

    public IReadOnlyList<T> Data { get; init; } = [];

    public PaginationMetadata Pagination { get; init; } = new(1, 20, 0, 0);

    public string? TraceId { get; init; } = Activity.Current?.TraceId.ToString();

    public static PagedResponse<T> From(PagedResult<T> result, string? message = null)
    {
        return new PagedResponse<T>
        {
            Data = result.Items,
            Message = message,
            Pagination = new PaginationMetadata(
                result.Page,
                result.PageSize,
                result.TotalCount,
                result.TotalPages)
        };
    }
}

public sealed record PaginationMetadata(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
