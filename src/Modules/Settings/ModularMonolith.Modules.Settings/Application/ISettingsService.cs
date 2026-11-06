using ModularMonolith.Modules.Settings.Application.Dtos;
using ModularMonolith.Modules.Settings.Application.Requests;
using ModularMonolith.SharedKernel.Pagination;

namespace ModularMonolith.Modules.Settings.Application;

public interface ISettingsService
{
    Task<PagedResult<SettingDto>> ListAsync(
        SettingsQuery query,
        CancellationToken cancellationToken = default);

    Task<SettingDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SettingDto> CreateAsync(
        CreateSettingRequest request,
        CancellationToken cancellationToken = default);

    Task<SettingDto> UpdateAsync(
        Guid id,
        UpdateSettingRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<SettingHistoryDto>> GetHistoryAsync(
        Guid id,
        PagedRequest request,
        CancellationToken cancellationToken = default);
}
