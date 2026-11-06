using System.Text.RegularExpressions;
using ModularMonolith.SharedKernel.Exceptions;

namespace ModularMonolith.Modules.Settings.Domain;

public static class SettingRules
{
    public const int MaxKeyLength = 200;
    public const int MaxValueLength = 4000;
    public const int MaxDescriptionLength = 500;
    public const int MaxCategoryLength = 100;
    public const int MaxModuleLength = 100;
    public const int MaxValidationPatternLength = 500;
    public const int MaxAllowedValues = 100;

    private static readonly Regex KeyPattern = new(
        "^[A-Za-z0-9][A-Za-z0-9._:-]*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    private static readonly string[] SecretKeyFragments =
    [
        "password",
        "secret",
        "connectionstring",
        "connection-string",
        "apikey",
        "api-key",
        "privatekey",
        "private-key",
        "clientsecret",
        "client-secret"
    ];

    public static string NormalizeKey(string key)
    {
        return key.Trim().ToUpperInvariant();
    }

    public static bool IsValidKeyFormat(string? key)
    {
        return !string.IsNullOrWhiteSpace(key)
            && key.Trim().Length <= MaxKeyLength
            && KeyPattern.IsMatch(key.Trim());
    }

    public static bool ContainsSecretFragment(string? key)
    {
        return !string.IsNullOrWhiteSpace(key)
            && SecretKeyFragments.Any(fragment =>
                key.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    public static void EnsureValidKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainException("A setting key is required.", "settings.key_required");
        }

        var trimmed = key.Trim();

        if (trimmed.Length > MaxKeyLength)
        {
            throw new DomainException(
                $"A setting key cannot exceed {MaxKeyLength} characters.",
                "settings.key_too_long");
        }

        if (!KeyPattern.IsMatch(trimmed))
        {
            throw new DomainException(
                "A setting key may only contain letters, digits, dots, dashes, underscores, and colons, and must start with a letter or digit.",
                "settings.key_invalid");
        }

        if (ContainsSecretFragment(trimmed))
        {
            throw new DomainException(
                "Secrets must not be stored in application settings. Use the secret manager or environment configuration instead.",
                "settings.secret_not_allowed");
        }
    }

    public static void EnsureValidModule(string? module, SettingScope scope)
    {
        if (scope == SettingScope.Module && string.IsNullOrWhiteSpace(module))
        {
            throw new DomainException(
                "A module name is required for module-scoped settings.",
                "settings.module_required");
        }

        if (module is not null && module.Length > MaxModuleLength)
        {
            throw new DomainException(
                $"A module name cannot exceed {MaxModuleLength} characters.",
                "settings.module_too_long");
        }
    }

    public static void EnsureScopeOwner(
        SettingScope scope,
        Guid tenantId,
        Guid userId)
    {
        switch (scope)
        {
            case SettingScope.Global:
            case SettingScope.Module:
                if (tenantId != Guid.Empty || userId != Guid.Empty)
                {
                    throw new DomainException(
                        "Global and module settings cannot be owned by a tenant or a user.",
                        "settings.scope_owner_invalid");
                }

                break;
            case SettingScope.Tenant:
                if (tenantId == Guid.Empty)
                {
                    throw new DomainException(
                        "A tenant identifier is required for tenant-scoped settings.",
                        "settings.tenant_required");
                }

                if (userId != Guid.Empty)
                {
                    throw new DomainException(
                        "Tenant settings cannot be owned by a user.",
                        "settings.scope_owner_invalid");
                }

                break;
            case SettingScope.User:
                if (userId == Guid.Empty)
                {
                    throw new DomainException(
                        "A user identifier is required for user-scoped settings.",
                        "settings.user_required");
                }

                break;
            default:
                throw new DomainException(
                    "The setting scope is not supported.",
                    "settings.scope_invalid");
        }
    }
}
