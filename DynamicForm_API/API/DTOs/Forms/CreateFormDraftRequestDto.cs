namespace DynamicForm_API.API.DTOs.Forms
{
    public class CreateFormDraftRequestDto
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
    }
}
