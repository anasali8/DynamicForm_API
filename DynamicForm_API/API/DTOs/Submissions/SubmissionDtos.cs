using DynamicForm_API.Shared.Enums;
using System.Text.Json;

namespace DynamicForm_API.API.DTOs.Submissions
{
    public class SubmitFormVersionRequestDto
    {
        public string UserId { get; set; } = null!;
        public Dictionary<string, JsonElement> SubmissionData { get; set; } = new();
    }

    public class FormSubmissionCreatedResponseDto
    {
        public int SubmissionId { get; set; }
        public int FormId { get; set; }
        public int FormVersionId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
    }

    public class FormSubmissionListItemResponseDto
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public int FormVersionId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
    }

    public class FormSubmissionDetailResponseDto
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public int FormVersionId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public IReadOnlyList<FormSubmissionGroupResponseDto> Groups { get; set; } = Array.Empty<FormSubmissionGroupResponseDto>();
    }

    public class FormSubmissionGroupResponseDto
    {
        public string? GroupKey { get; set; }
        public IReadOnlyList<FormSubmissionFieldResponseDto> Fields { get; set; } = Array.Empty<FormSubmissionFieldResponseDto>();
    }

    public class FormSubmissionFieldResponseDto
    {
        public string Name { get; set; } = null!;
        public string Label { get; set; } = null!;
        public FieldType Type { get; set; }
        public string? GroupKey { get; set; }
        public string? BindingKey { get; set; }
        public int Order { get; set; }
        public object? RawValue { get; set; }
        public string? DisplayValue { get; set; }
    }
}
