using ModularMonolith.SharedKernel.Domain;
using ModularMonolith.SharedKernel.Exceptions;

namespace ModularMonolith.Modules.Settings.Domain;

public sealed class Setting : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    private Setting()
    {
    }

    private Setting(
        Guid id,
        string key,
        string normalizedKey,
        string? value,
        SettingDataType dataType,
        SettingScope scope,
        string module,
        Guid tenantId,
        Guid userId,
        string? description,
        string? category,
        bool isRequired,
        bool isReadOnly,
        string? allowedValuesJson,
        string? validationPattern,
        DateTimeOffset createdAt)
        : base(id)
    {
        Key = key;
        NormalizedKey = normalizedKey;
        Value = value;
        DataType = dataType;
        Scope = scope;
        Module = module;
        TenantId = tenantId;
        UserId = userId;
        Description = description;
        Category = category;
        IsRequired = isRequired;
        IsReadOnly = isReadOnly;
        AllowedValuesJson = allowedValuesJson;
        ValidationPattern = validationPattern;
        Version = 1;
        CreatedAt = createdAt;
    }

    public string Key { get; private set; } = string.Empty;

    public string NormalizedKey { get; private set; } = string.Empty;

    public string? Value { get; private set; }

    public SettingDataType DataType { get; private set; }

    public SettingScope Scope { get; private set; }

    public string Module { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public string? Description { get; private set; }

    public string? Category { get; private set; }

    public bool IsRequired { get; private set; }

    public bool IsReadOnly { get; private set; }

    public string? AllowedValuesJson { get; private set; }

    public string? ValidationPattern { get; private set; }

    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public static Setting Create(
        string key,
        string? value,
        SettingDataType dataType,
        SettingScope scope,
        string? module,
        Guid tenantId,
        Guid userId,
        string? description,
        string? category,
        bool isRequired,
        bool isReadOnly,
        string? allowedValuesJson,
        string? validationPattern,
        DateTimeOffset createdAt)
    {
        SettingRules.EnsureValidKey(key);
        SettingRules.EnsureValidModule(module, scope);
        SettingRules.EnsureScopeOwner(scope, tenantId, userId);
        EnsureTextLength(description, SettingRules.MaxDescriptionLength, "description");
        EnsureTextLength(category, SettingRules.MaxCategoryLength, "category");

        if (allowedValuesJson is not null && allowedValuesJson.Length > SettingRules.MaxValueLength)
        {
            throw new DomainException(
                "The allowed values configuration is too large.",
                "settings.allowed_values_too_large");
        }

        EnsureValidValue(dataType, value, isRequired, allowedValuesJson, validationPattern);

        var trimmedKey = key.Trim();
        var normalizedModule = module?.Trim() ?? string.Empty;

        return new Setting(
            Guid.CreateVersion7(),
            trimmedKey,
            SettingRules.NormalizeKey(trimmedKey),
            value,
            dataType,
            scope,
            normalizedModule,
            tenantId,
            userId,
            description?.Trim(),
            category?.Trim(),
            isRequired,
            isReadOnly,
            allowedValuesJson,
            validationPattern,
            createdAt);
    }

    public void Update(
        string? value,
        string? description,
        string? category,
        bool isRequired,
        bool isReadOnly,
        string? allowedValuesJson,
        string? validationPattern)
    {
        if (IsReadOnly)
        {
            throw new DomainException(
                $"Setting '{Key}' is read-only and cannot be modified.",
                "settings.read_only");
        }

        EnsureTextLength(description, SettingRules.MaxDescriptionLength, "description");
        EnsureTextLength(category, SettingRules.MaxCategoryLength, "category");

        if (allowedValuesJson is not null && allowedValuesJson.Length > SettingRules.MaxValueLength)
        {
            throw new DomainException(
                "The allowed values configuration is too large.",
                "settings.allowed_values_too_large");
        }

        EnsureValidValue(DataType, value, isRequired, allowedValuesJson, validationPattern);

        Value = value;
        Description = description?.Trim();
        Category = category?.Trim();
        IsRequired = isRequired;
        IsReadOnly = isReadOnly;
        AllowedValuesJson = allowedValuesJson;
        ValidationPattern = validationPattern;
        Version++;
    }

    public bool HasSameState(
        string? value,
        string? description,
        string? category,
        bool isRequired,
        bool isReadOnly,
        string? allowedValuesJson,
        string? validationPattern)
    {
        return string.Equals(Value, value, StringComparison.Ordinal)
            && string.Equals(Description, description?.Trim(), StringComparison.Ordinal)
            && string.Equals(Category, category?.Trim(), StringComparison.Ordinal)
            && IsRequired == isRequired
            && IsReadOnly == isReadOnly
            && string.Equals(AllowedValuesJson, allowedValuesJson, StringComparison.Ordinal)
            && string.Equals(ValidationPattern, validationPattern, StringComparison.Ordinal);
    }

    private static void EnsureValidValue(
        SettingDataType dataType,
        string? value,
        bool isRequired,
        string? allowedValuesJson,
        string? validationPattern)
    {
        var errors = SettingValueValidator.Validate(
            dataType,
            value,
            isRequired,
            allowedValuesJson,
            validationPattern);

        if (errors.Count > 0)
        {
            throw new DomainException(
                string.Join(" ", errors.Values.SelectMany(valueErrors => valueErrors)),
                "settings.value_invalid");
        }
    }

    private static void EnsureTextLength(string? value, int maxLength, string fieldName)
    {
        if (value is not null && value.Length > maxLength)
        {
            throw new DomainException(
                $"The setting {fieldName} cannot exceed {maxLength} characters.",
                $"settings.{fieldName}_too_long");
        }
    }
}
