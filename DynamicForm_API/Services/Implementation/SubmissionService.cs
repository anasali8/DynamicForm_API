using AutoMapper;
using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Submissions;
using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Services.Interfaces;
using DynamicForm_API.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DynamicForm_API.Services.Implementation
{
    public class SubmissionService : ISubmissionService
    {
        private static readonly HashSet<FieldType> TextLikeFieldTypes =
        [
            FieldType.Text,
            FieldType.Textarea,
            FieldType.Email,
            FieldType.Phone
        ];

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubmissionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FormSubmissionCreatedResponseDto> SubmitFormAsync(string formCode, SubmitFormVersionRequestDto request, CancellationToken cancellationToken = default)
        {
            var form = await _unitOfWork.Forms.GetByCodeAsync(formCode, cancellationToken)
                ?? throw new KeyNotFoundException($"Form with code '{formCode}' was not found.");

            var currentPublishedVersion = await ResolveCurrentPublishedVersionAsync(form.Id, cancellationToken);
            await ValidateSubmissionPayloadAsync(request.SubmissionData, currentPublishedVersion.Fields, cancellationToken);

            var payloadJson = JsonSerializer.Serialize(request.SubmissionData);
            var submission = new FormSubmission
            {
                FormId = form.Id,
                FormVersionId = currentPublishedVersion.Id,
                UserId = request.UserId,
                SubmissionData = payloadJson,
                SubmittedAt = DateTime.UtcNow
            };

            await _unitOfWork.Submissions.AddAsync(submission, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<FormSubmissionCreatedResponseDto>(submission);
        }

        public async Task<PagedResponseDto<FormSubmissionListItemResponseDto>> GetFormSubmissionsAsync(string formCode, PagedRequestDto request, string? userId = null, CancellationToken cancellationToken = default)
        {
            var form = await _unitOfWork.Forms.GetByCodeAsync(formCode, cancellationToken)
                ?? throw new KeyNotFoundException($"Form with code '{formCode}' was not found.");

            var items = await _unitOfWork.Submissions.GetByFormIdAsync(form.Id, request.PageNumber, request.PageSize, userId, cancellationToken);

            var totalQuery = _unitOfWork.Submissions.GetQueryable()
                .AsNoTracking()
                .Where(x => x.FormId == form.Id);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                totalQuery = totalQuery.Where(x => x.UserId == userId);
            }

            var totalCount = await totalQuery.CountAsync(cancellationToken);

            return new PagedResponseDto<FormSubmissionListItemResponseDto>
            {
                Items = _mapper.Map<IReadOnlyList<FormSubmissionListItemResponseDto>>(items),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<FormSubmissionDetailResponseDto> GetSubmissionDetailsAsync(int formId, int submissionId, CancellationToken cancellationToken = default)
        {
            await EnsureFormExistsAsync(formId, cancellationToken);

            var submission = await _unitOfWork.Submissions.GetDetailsByIdWithIncludesAsync(submissionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Submission '{submissionId}' was not found.");

            if (submission.FormId != formId)
            {
                throw new KeyNotFoundException($"Submission '{submissionId}' was not found for form '{formId}'.");
            }

            return await BuildSubmissionDetailResponseAsync(submission, cancellationToken);
        }

        public async Task<FormSubmissionDetailResponseDto?> GetSubmissionDetailsByIdAsync(int submissionId, CancellationToken cancellationToken = default)
        {
            var submission = await _unitOfWork.Submissions.GetDetailsByIdWithIncludesAsync(submissionId, cancellationToken);
            return submission is null ? null : await BuildSubmissionDetailResponseAsync(submission, cancellationToken);
        }

        private async Task<FormSubmissionDetailResponseDto> BuildSubmissionDetailResponseAsync(FormSubmission submission, CancellationToken cancellationToken)
        {
            var payload = DeserializeSubmissionData(submission.SubmissionData);
            var fields = submission.FormVersion.Fields
                .OrderBy(x => x.Order)
                .ToList();

            var groupAccumulator = new List<(string? GroupKey, List<FormSubmissionFieldResponseDto> Fields)>();
            var lookupDisplayCache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in fields)
            {
                payload.TryGetValue(field.Name, out var rawElement);

                var rawValue = rawElement.ValueKind == JsonValueKind.Undefined
                    ? null
                    : ConvertJsonElementToObject(rawElement);

                var displayValue = await ResolveDisplayValueAsync(field, rawElement, lookupDisplayCache, cancellationToken);

                var fieldDto = new FormSubmissionFieldResponseDto
                {
                    Name = field.Name,
                    Label = field.Label,
                    Type = field.Type,
                    GroupKey = field.GroupKey,
                    BindingKey = field.BindingKey,
                    Order = field.Order,
                    RawValue = rawValue,
                    DisplayValue = displayValue
                };

                var existingGroup = groupAccumulator.FirstOrDefault(x => x.GroupKey == field.GroupKey);
                if (existingGroup.Fields is null)
                {
                    groupAccumulator.Add((field.GroupKey, [fieldDto]));
                }
                else
                {
                    existingGroup.Fields.Add(fieldDto);
                }
            }

            return new FormSubmissionDetailResponseDto
            {
                Id = submission.Id,
                FormId = submission.FormId,
                FormVersionId = submission.FormVersionId,
                UserId = submission.UserId,
                SubmittedAt = submission.SubmittedAt,
                Groups = groupAccumulator
                    .Select(x => new FormSubmissionGroupResponseDto
                    {
                        GroupKey = x.GroupKey,
                        Fields = x.Fields
                    })
                    .ToList()
            };
        }

        private async Task<string?> ResolveDisplayValueAsync(
            FormField field,
            JsonElement rawElement,
            Dictionary<string, Dictionary<string, string>> lookupDisplayCache,
            CancellationToken cancellationToken)
        {
            if (rawElement.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                return null;
            }

            var parsedConfig = FieldConfigHelper.Parse(field.ConfigJson);

            switch (field.Type)
            {
                case FieldType.Select:
                case FieldType.Radio:
                {
                    var rawValue = ExtractString(rawElement);
                    if (string.IsNullOrWhiteSpace(rawValue))
                    {
                        return null;
                    }

                    var label = await ResolveOptionLabelAsync(rawValue, parsedConfig, lookupDisplayCache, cancellationToken);
                    return label ?? rawValue;
                }
                case FieldType.MultiSelect:
                {
                    if (rawElement.ValueKind != JsonValueKind.Array)
                    {
                        return rawElement.ToString();
                    }

                    var values = rawElement.EnumerateArray()
                        .Select(ExtractString)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Cast<string>()
                        .ToList();

                    var labels = new List<string>();
                    foreach (var value in values)
                    {
                        var label = await ResolveOptionLabelAsync(value, parsedConfig, lookupDisplayCache, cancellationToken);
                        labels.Add(label ?? value);
                    }

                    return string.Join(", ", labels);
                }
                default:
                    return rawElement.ValueKind switch
                    {
                        JsonValueKind.String => rawElement.GetString(),
                        JsonValueKind.True => bool.TrueString,
                        JsonValueKind.False => bool.FalseString,
                        _ => rawElement.ToString()
                    };
            }
        }

        private async Task<string?> ResolveOptionLabelAsync(
            string rawValue,
            ParsedFieldConfig config,
            Dictionary<string, Dictionary<string, string>> lookupDisplayCache,
            CancellationToken cancellationToken)
        {
            if (config.OptionsMode == OptionsMode.Static && config.StaticOptions.Count > 0)
            {
                return config.StaticOptions.FirstOrDefault(x => x.Value == rawValue)?.Label;
            }

            if (config.OptionsMode == OptionsMode.Lookup && !string.IsNullOrWhiteSpace(config.LookupKey))
            {
                var cache = await GetLookupDisplayMapAsync(config.LookupKey, lookupDisplayCache, cancellationToken);
                if (cache.TryGetValue(rawValue, out var label))
                {
                    return label;
                }
            }

            return null;
        }

        private async Task<Dictionary<string, string>> GetLookupDisplayMapAsync(
            string lookupKey,
            Dictionary<string, Dictionary<string, string>> cache,
            CancellationToken cancellationToken)
        {
            if (cache.TryGetValue(lookupKey, out var existing))
            {
                return existing;
            }

            var activeItems = await _unitOfWork.Lookups.GetActiveItemsAsync(lookupKey, cancellationToken);
            var map = activeItems
                .GroupBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.First().Label, StringComparer.Ordinal);

            cache[lookupKey] = map;
            return map;
        }

        private static Dictionary<string, JsonElement> DeserializeSubmissionData(string submissionData)
        {
            if (string.IsNullOrWhiteSpace(submissionData))
            {
                return new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            }

            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(submissionData)
                ?? new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }

        private async Task EnsureFormExistsAsync(int formId, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.Forms.GetQueryable()
                .AsNoTracking()
                .AnyAsync(x => x.Id == formId, cancellationToken);

            if (!exists)
            {
                throw new KeyNotFoundException($"Form with id '{formId}' was not found.");
            }
        }

        private async Task<FormVersion> ResolveCurrentPublishedVersionAsync(int formId, CancellationToken cancellationToken)
        {
            var version = await _unitOfWork.FormVersions.GetCurrentPublishedVersionAsync(formId, cancellationToken);
            if (version is null)
            {
                throw new InvalidOperationException($"No current published version exists for form '{formId}'.");
            }

            return version;
        }

        private async Task ValidateSubmissionPayloadAsync(Dictionary<string, JsonElement> submissionData, ICollection<FormField> versionFields, CancellationToken cancellationToken)
        {
            if (submissionData.Count == 0)
            {
                throw new ArgumentException("Submission data cannot be empty.", nameof(submissionData));
            }

            var knownFields = versionFields.ToDictionary(x => x.Name, x => x, StringComparer.Ordinal);

            var unknownKeys = submissionData.Keys.Where(x => !knownFields.ContainsKey(x)).ToList();
            if (unknownKeys.Count > 0)
            {
                throw new ArgumentException($"Unknown field keys: {string.Join(", ", unknownKeys)}", nameof(submissionData));
            }

            var lookupValidationCache = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in versionFields)
            {
                var hasValue = submissionData.TryGetValue(field.Name, out var value);
                if (!hasValue)
                {
                    if (field.IsRequired)
                    {
                        throw new ArgumentException($"Missing required field values: {field.Name}", nameof(submissionData));
                    }

                    continue;
                }

                if (field.IsRequired && IsMissingRequiredValue(value))
                {
                    throw new ArgumentException($"Missing required field values: {field.Name}", nameof(submissionData));
                }

                if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                {
                    continue;
                }

                var parsedConfig = FieldConfigHelper.Parse(field.ConfigJson);
                await ValidateFieldValueAsync(field, value, parsedConfig, lookupValidationCache, cancellationToken);
            }
        }

        private async Task ValidateFieldValueAsync(
            FormField field,
            JsonElement value,
            ParsedFieldConfig config,
            Dictionary<string, HashSet<string>> lookupValidationCache,
            CancellationToken cancellationToken)
        {
            switch (field.Type)
            {
                case FieldType.Text:
                case FieldType.Textarea:
                    EnsureJsonKind(field, value, JsonValueKind.String, "string");
                    break;
                case FieldType.Number:
                    EnsureNumber(field, value, config);
                    break;
                case FieldType.Date:
                    EnsureDate(field, value, config);
                    break;
                case FieldType.Checkbox:
                    EnsureJsonKind(field, value, JsonValueKind.True, "boolean", allowAlternativeBoolKind: true);
                    break;
                case FieldType.Email:
                    EnsureEmail(field, value);
                    break;
                case FieldType.Phone:
                    EnsurePhone(field, value);
                    break;
                case FieldType.Select:
                case FieldType.Radio:
                    await EnsureSingleOptionMembershipAsync(field, value, config, lookupValidationCache, cancellationToken);
                    break;
                case FieldType.MultiSelect:
                    await EnsureMultiOptionMembershipAsync(field, value, config, lookupValidationCache, cancellationToken);
                    break;
                default:
                    break;
            }

            if (TextLikeFieldTypes.Contains(field.Type) && !string.IsNullOrWhiteSpace(field.RegexPattern))
            {
                var text = ExtractString(value) ?? string.Empty;
                if (!Regex.IsMatch(text, field.RegexPattern))
                {
                    throw new ArgumentException($"Field '{field.Name}' does not match regex validation.");
                }
            }
        }

        private static void EnsureJsonKind(FormField field, JsonElement value, JsonValueKind expected, string expectedType, bool allowAlternativeBoolKind = false)
        {
            var isBool = value.ValueKind is JsonValueKind.True or JsonValueKind.False;
            if (allowAlternativeBoolKind && isBool)
            {
                return;
            }

            if (value.ValueKind != expected)
            {
                throw new ArgumentException($"Field '{field.Name}' must be a valid {expectedType}.");
            }
        }

        private static void EnsureNumber(FormField field, JsonElement value, ParsedFieldConfig config)
        {
            if (value.ValueKind != JsonValueKind.Number)
            {
                throw new ArgumentException($"Field '{field.Name}' must be a valid number.");
            }

            if (!value.TryGetDecimal(out var numberValue))
            {
                throw new ArgumentException($"Field '{field.Name}' must be a valid number.");
            }

            if (config.MinNumber.HasValue && numberValue < config.MinNumber.Value)
            {
                throw new ArgumentException($"Field '{field.Name}' must be >= {config.MinNumber.Value.ToString(CultureInfo.InvariantCulture)}.");
            }

            if (config.MaxNumber.HasValue && numberValue > config.MaxNumber.Value)
            {
                throw new ArgumentException($"Field '{field.Name}' must be <= {config.MaxNumber.Value.ToString(CultureInfo.InvariantCulture)}.");
            }
        }

        private static void EnsureDate(FormField field, JsonElement value, ParsedFieldConfig config)
        {
            if (value.ValueKind != JsonValueKind.String || !DateTime.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateValue))
            {
                throw new ArgumentException($"Field '{field.Name}' must be a valid date.");
            }

            if (config.MinDate.HasValue && dateValue < config.MinDate.Value)
            {
                throw new ArgumentException($"Field '{field.Name}' must be on/after {config.MinDate.Value:O}.");
            }

            if (config.MaxDate.HasValue && dateValue > config.MaxDate.Value)
            {
                throw new ArgumentException($"Field '{field.Name}' must be on/before {config.MaxDate.Value:O}.");
            }
        }

        private static void EnsureEmail(FormField field, JsonElement value)
        {
            EnsureJsonKind(field, value, JsonValueKind.String, "email");
            var email = value.GetString();
            var validator = new EmailAddressAttribute();
            if (string.IsNullOrWhiteSpace(email) || !validator.IsValid(email))
            {
                throw new ArgumentException($"Field '{field.Name}' must be a valid email address.");
            }
        }

        private static void EnsurePhone(FormField field, JsonElement value)
        {
            EnsureJsonKind(field, value, JsonValueKind.String, "phone");
            var phone = value.GetString() ?? string.Empty;
            if (!Regex.IsMatch(phone, @"^\+?[0-9()\-\s]{7,20}$"))
            {
                throw new ArgumentException($"Field '{field.Name}' must be a valid phone number.");
            }
        }

        private async Task EnsureSingleOptionMembershipAsync(
            FormField field,
            JsonElement value,
            ParsedFieldConfig config,
            Dictionary<string, HashSet<string>> lookupValidationCache,
            CancellationToken cancellationToken)
        {
            EnsureJsonKind(field, value, JsonValueKind.String, "string option value");
            var selected = value.GetString() ?? string.Empty;

            if (config.OptionsMode == OptionsMode.Static && config.StaticOptions.Count > 0)
            {
                if (!config.StaticOptions.Any(x => x.Value == selected))
                {
                    throw new ArgumentException($"Field '{field.Name}' has an invalid option value.");
                }

                return;
            }

            if (config.OptionsMode == OptionsMode.Lookup && !string.IsNullOrWhiteSpace(config.LookupKey))
            {
                var allowed = await GetLookupAllowedValuesAsync(config.LookupKey, config.ActiveOnly, lookupValidationCache, cancellationToken);
                if (!allowed.Contains(selected))
                {
                    throw new ArgumentException($"Field '{field.Name}' has an invalid lookup value.");
                }
            }
        }

        private async Task EnsureMultiOptionMembershipAsync(
            FormField field,
            JsonElement value,
            ParsedFieldConfig config,
            Dictionary<string, HashSet<string>> lookupValidationCache,
            CancellationToken cancellationToken)
        {
            if (value.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException($"Field '{field.Name}' must be an array.");
            }

            var selectedValues = value.EnumerateArray().Select(ExtractString).ToList();
            if (selectedValues.Any(x => string.IsNullOrWhiteSpace(x)))
            {
                throw new ArgumentException($"Field '{field.Name}' contains invalid option value(s).");
            }

            if (config.OptionsMode == OptionsMode.Static && config.StaticOptions.Count > 0)
            {
                var allowed = config.StaticOptions.Select(x => x.Value).ToHashSet(StringComparer.Ordinal);
                if (selectedValues.Any(x => x is not null && !allowed.Contains(x)))
                {
                    throw new ArgumentException($"Field '{field.Name}' has invalid option value(s).");
                }

                return;
            }

            if (config.OptionsMode == OptionsMode.Lookup && !string.IsNullOrWhiteSpace(config.LookupKey))
            {
                var allowed = await GetLookupAllowedValuesAsync(config.LookupKey, config.ActiveOnly, lookupValidationCache, cancellationToken);
                if (selectedValues.Any(x => x is not null && !allowed.Contains(x)))
                {
                    throw new ArgumentException($"Field '{field.Name}' has invalid lookup value(s).");
                }
            }
        }

        private async Task<HashSet<string>> GetLookupAllowedValuesAsync(
            string lookupKey,
            bool activeOnly,
            Dictionary<string, HashSet<string>> cache,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"{lookupKey}::{activeOnly}";
            if (cache.TryGetValue(cacheKey, out var existing))
            {
                return existing;
            }

            HashSet<string> values;
            if (activeOnly)
            {
                var activeItems = await _unitOfWork.Lookups.GetActiveItemsAsync(lookupKey, cancellationToken);
                values = activeItems.Select(x => x.Value).ToHashSet(StringComparer.Ordinal);
            }
            else
            {
                var table = await _unitOfWork.Lookups.GetByCodeAsync(lookupKey, cancellationToken);
                values = table?.Items.Select(x => x.Value).ToHashSet(StringComparer.Ordinal)
                    ?? new HashSet<string>(StringComparer.Ordinal);
            }

            cache[cacheKey] = values;
            return values;
        }

        private static bool IsMissingRequiredValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Null => true,
                JsonValueKind.Undefined => true,
                JsonValueKind.String => string.IsNullOrWhiteSpace(value.GetString()),
                JsonValueKind.Array => !value.EnumerateArray().Any(),
                _ => false
            };
        }

        private static string? ExtractString(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
        }

        private static object? ConvertJsonElementToObject(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var l)
                    ? l
                    : element.TryGetDecimal(out var d)
                        ? d
                        : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElementToObject).ToList(),
                JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText()),
                _ => null
            };
        }
    }
}
