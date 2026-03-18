using DynamicForm_API.Shared.Enums;
using System.Globalization;
using System.Text.Json;

namespace DynamicForm_API.Services.Implementation
{
    internal sealed class ParsedFieldConfig
    {
        public OptionsMode? OptionsMode { get; init; }
        public string? LookupKey { get; init; }
        public bool ActiveOnly { get; init; } = true;
        public decimal? MinNumber { get; init; }
        public decimal? MaxNumber { get; init; }
        public DateTime? MinDate { get; init; }
        public DateTime? MaxDate { get; init; }
        public IReadOnlyList<ParsedOptionItem> StaticOptions { get; init; } = Array.Empty<ParsedOptionItem>();
    }

    internal sealed class ParsedOptionItem
    {
        public string Value { get; init; } = null!;
        public string Label { get; init; } = null!;
        public int Order { get; init; }
        public string? ParentValue { get; init; }
    }

    internal static class FieldConfigHelper
    {
        public static ParsedFieldConfig Parse(string? configJson)
        {
            if (string.IsNullOrWhiteSpace(configJson))
            {
                return new ParsedFieldConfig();
            }

            try
            {
                using var document = JsonDocument.Parse(configJson);
                var root = document.RootElement;

                var optionsMode = TryParseOptionsMode(root);
                var lookupKey = TryGetString(root, "lookupKey");
                var activeOnly = TryGetBool(root, "activeOnly") ?? true;
                var minNumber = TryGetDecimal(root, "min") ?? TryGetDecimal(root, "minValue");
                var maxNumber = TryGetDecimal(root, "max") ?? TryGetDecimal(root, "maxValue");
                var minDate = TryGetDateTime(root, "min") ?? TryGetDateTime(root, "minDate") ?? TryGetDateTime(root, "minValue");
                var maxDate = TryGetDateTime(root, "max") ?? TryGetDateTime(root, "maxDate") ?? TryGetDateTime(root, "maxValue");

                var staticOptions = ParseOptions(root)
                    .OrderBy(x => x.Order)
                    .ToList();

                return new ParsedFieldConfig
                {
                    OptionsMode = optionsMode,
                    LookupKey = lookupKey,
                    ActiveOnly = activeOnly,
                    MinNumber = minNumber,
                    MaxNumber = maxNumber,
                    MinDate = minDate,
                    MaxDate = maxDate,
                    StaticOptions = staticOptions
                };
            }
            catch
            {
                return new ParsedFieldConfig();
            }
        }

        private static OptionsMode? TryParseOptionsMode(JsonElement root)
        {
            if (!TryGetProperty(root, "optionsMode", out var element))
            {
                return null;
            }

            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var numeric) && Enum.IsDefined(typeof(OptionsMode), numeric))
            {
                return (OptionsMode)numeric;
            }

            if (element.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var mode = element.GetString();
            if (string.IsNullOrWhiteSpace(mode))
            {
                return null;
            }

            if (Enum.TryParse<OptionsMode>(mode, true, out var enumMode))
            {
                return enumMode;
            }

            return mode.Equals("lookup", StringComparison.OrdinalIgnoreCase)
                ? OptionsMode.Lookup
                : mode.Equals("static", StringComparison.OrdinalIgnoreCase)
                    ? OptionsMode.Static
                    : null;
        }

        private static IEnumerable<ParsedOptionItem> ParseOptions(JsonElement root)
        {
            if (!TryGetProperty(root, "options", out var optionsElement) && !TryGetProperty(root, "items", out optionsElement))
            {
                yield break;
            }

            if (optionsElement.ValueKind != JsonValueKind.Array)
            {
                yield break;
            }

            var order = 1;
            foreach (var item in optionsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var value = TryGetString(item, "value");
                var label = TryGetString(item, "label");
                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                yield return new ParsedOptionItem
                {
                    Value = value,
                    Label = label,
                    Order = TryGetInt(item, "order") ?? order,
                    ParentValue = TryGetString(item, "parentValue")
                };
                order++;
            }
        }

        private static bool TryGetProperty(JsonElement root, string name, out JsonElement value)
        {
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out value))
            {
                return true;
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in root.EnumerateObject())
                {
                    if (property.NameEquals(name) || property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        private static string? TryGetString(JsonElement root, string propertyName)
        {
            if (!TryGetProperty(root, propertyName, out var value))
            {
                return null;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
        }

        private static bool? TryGetBool(JsonElement root, string propertyName)
        {
            if (!TryGetProperty(root, propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.True)
            {
                return true;
            }

            if (value.ValueKind == JsonValueKind.False)
            {
                return false;
            }

            if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out var result))
            {
                return result;
            }

            return null;
        }

        private static decimal? TryGetDecimal(JsonElement root, string propertyName)
        {
            if (!TryGetProperty(root, propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var decimalValue))
            {
                return decimalValue;
            }

            if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static DateTime? TryGetDateTime(JsonElement root, string propertyName)
        {
            if (!TryGetProperty(root, propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.String && DateTime.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            {
                return date;
            }

            return null;
        }

        private static int? TryGetInt(JsonElement root, string propertyName)
        {
            if (!TryGetProperty(root, propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intValue))
            {
                return intValue;
            }

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
