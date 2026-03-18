using DynamicForm_API.API.DTOs.Versions;

namespace DynamicForm_API.API.DTOs.Forms
{
    public class FormDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IReadOnlyList<FormVersionSummaryResponseDto> Versions { get; set; } = Array.Empty<FormVersionSummaryResponseDto>();
    }
}
