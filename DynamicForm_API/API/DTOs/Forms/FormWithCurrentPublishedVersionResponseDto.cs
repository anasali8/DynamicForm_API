using DynamicForm_API.API.DTOs.Versions;

namespace DynamicForm_API.API.DTOs.Forms
{
    public class FormWithCurrentPublishedVersionResponseDto
    {
        public int FormId { get; set; }
        public string FormName { get; set; } = null!;
        public string FormCode { get; set; } = null!;
        public FormVersionDetailResponseDto CurrentPublishedVersion { get; set; } = null!;
    }
}
