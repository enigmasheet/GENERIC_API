using System.Globalization;
using System.Text.Json;
using ModularMonolith.Modules.Settings.Domain;

namespace ModularMonolith.Modules.Settings.Application;

public static class SettingValueConverter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static T? Convert<T>(string? value, SettingDataType dataType)
    {
        if (value is null)
        {
            return default;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (targetType == typeof(string))
        {
            return (T)(object)value;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        if (targetType == typeof(Guid))
        {
            return (T)(object)Guid.Parse(value);
        }

        if (targetType == typeof(DateTimeOffset))
        {
            return (T)(object)DateTimeOffset.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        if (targetType == typeof(DateTime))
        {
            return (T)(object)DateTime.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        if (targetType.IsEnum)
        {
            return (T)Enum.Parse(targetType, value, ignoreCase: true);
        }

        if (targetType == typeof(bool)
            || targetType == typeof(byte)
            || targetType == typeof(short)
            || targetType == typeof(int)
            || targetType == typeof(long)
            || targetType == typeof(float)
            || targetType == typeof(double)
            || targetType == typeof(decimal))
        {
            return (T)System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        if (dataType == SettingDataType.Json || targetType.IsClass || targetType.IsValueType)
        {
            return JsonSerializer.Deserialize<T>(value, JsonOptions);
        }

        return default;
    }
}
