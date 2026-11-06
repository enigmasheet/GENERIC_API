
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ModularMonolith.Modules.Settings.Domain;

public static class SettingValueValidator
{
    public const string ValueField = "Value";

    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    public static IReadOnlyDictionary<string, string[]> Validate(
        SettingDataType dataType,
        string? value,
        bool isRequired,
        string? allowedValuesJson,
        string? validationPattern)
    {
        var failures = new List<string>();

        if (validationPattern is not null)
        {
            if (validationPattern.Length > SettingRules.MaxValidationPatternLength)
            {
                failures.Add(
                    $"The validation pattern cannot exceed {SettingRules.MaxValidationPatternLength} characters.");
            }
            else
            {
                try
                {
                    _ = new Regex(
                        validationPattern,
                        RegexOptions.CultureInvariant,
                        RegexTimeout);
                }
                catch (ArgumentException)
                {
                    failures.Add("The validation pattern is not a valid regular expression.");
                }
            }
        }

        string[]? allowedValues = null;

        if (!string.IsNullOrWhiteSpace(allowedValuesJson))
        {
            try
            {
                allowedValues = ParseAllowedValues(allowedValuesJson);
            }
            catch (JsonException)
            {
                failures.Add("The allowed values configuration is not valid JSON.");
            }
        }

        if (isRequired && string.IsNullOrWhiteSpace(value))
        {
            failures.Add("A value is required for this setting.");
        }
        else if (!string.IsNullOrEmpty(value))
        {
            if (value.Length > SettingRules.MaxValueLength)
            {
                failures.Add(
                    $"A setting value cannot exceed {SettingRules.MaxValueLength} characters.");
            }

            if (!IsValidValue(dataType, value))
            {
                failures.Add(DescribeDataType(dataType));
            }

            if (allowedValues is { Length: > 0 }
                && !allowedValues.Contains(value, StringComparer.Ordinal))
            {
                failures.Add("The value is not one of the allowed values.");
            }

            if (!string.IsNullOrEmpty(validationPattern)
                && !MatchesPattern(value, validationPattern))
            {
                failures.Add("The value does not match the configured validation pattern.");
            }
        }

        return failures.Count == 0
            ? new Dictionary<string, string[]>(StringComparer.Ordinal)
            : new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [ValueField] = [.. failures]
            };
    }

    public static bool IsValidValue(SettingDataType dataType, string value)
    {
        return dataType switch
        {
            SettingDataType.Text => true,

            SettingDataType.WholeNumber => long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out _),

            SettingDataType.Numeric => decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out _),

            SettingDataType.Flag => bool.TryParse(value, out _),

            SettingDataType.Json => IsValidJson(value),

            SettingDataType.DateTime => DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _),

            SettingDataType.UniqueIdentifier => Guid.TryParse(value, out _),

            _ => false
        };
    }

    public static string? SerializeAllowedValues(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return null;
        }

        var materialized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return materialized.Length == 0
            ? null
            : JsonSerializer.Serialize(materialized);
    }

    public static string[] ParseAllowedValues(string? allowedValuesJson)
    {
        if (string.IsNullOrWhiteSpace(allowedValuesJson))
        {
            return [];
        }

        return JsonSerializer.Deserialize<string[]>(allowedValuesJson) ?? [];
    }

    public static string DescribeDataType(SettingDataType dataType)
    {
        return dataType switch
        {
            SettingDataType.Text => "The value must be text.",
            SettingDataType.WholeNumber => "The value must be a whole number.",
            SettingDataType.Numeric => "The value must be a decimal number using invariant culture.",
            SettingDataType.Flag => "The value must be either 'true' or 'false'.",
            SettingDataType.Json => "The value must be valid JSON.",
            SettingDataType.DateTime => "The value must be an ISO 8601 date and time.",
            SettingDataType.UniqueIdentifier => "The value must be a valid unique identifier.",
            _ => "The value is not valid for the selected data type."
        };
    }

    private static bool IsValidJson(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.ValueKind is not JsonValueKind.Undefined;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        try
        {
            return Regex.IsMatch(
                value,
                pattern,
                RegexOptions.CultureInvariant,
                RegexTimeout);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
