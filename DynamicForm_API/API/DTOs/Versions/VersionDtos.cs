using DynamicForm_API.API.DTOs.Fields;
using DynamicForm_API.Shared.Enums;

namespace DynamicForm_API.API.DTOs.Versions
{
    public class CreateNextDraftVersionRequestDto
    {
        public int? SourceVersionId { get; set; }
    }

    public class UpdateDraftVersionRequestDto
    {
        public string? ChangeNote { get; set; }
    }

    public class PublishDraftVersionRequestDto
    {
        public string? PublishNote { get; set; }
    }

    public class ArchiveVersionRequestDto
    {
        public string? Reason { get; set; }
    }

    public class FormVersionSummaryResponseDto
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public int VersionNumber { get; set; }
        public FormVersionStatus Status { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class FormVersionHistoryItemResponseDto
    {
        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public FormVersionStatus Status { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FormVersionDetailResponseDto
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public int VersionNumber { get; set; }
        public FormVersionStatus Status { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IReadOnlyList<FormFieldResponseDto> Fields { get; set; } = Array.Empty<FormFieldResponseDto>();
    }
}
