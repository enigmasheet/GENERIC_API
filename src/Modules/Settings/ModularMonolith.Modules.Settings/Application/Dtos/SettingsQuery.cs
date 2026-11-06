using ModularMonolith.Modules.Settings.Domain;
using ModularMonolith.SharedKernel.Pagination;

namespace ModularMonolith.Modules.Settings.Application.Dtos;

public sealed class SettingsQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    public SettingScope? Scope { get; set; }

    public string? Module { get; set; }

    public string? Category { get; set; }

    public PagedRequest ToPagedRequest()
    {
        return new PagedRequest
        {
            Page = Page,
            PageSize = PageSize,
            Search = Search,
            SortBy = SortBy,
            SortDirection = SortDirection
        };
    }
}
