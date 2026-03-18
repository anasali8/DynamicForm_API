namespace DynamicForm_API.API.DTOs.Forms
{
    public class UpdateFormMetadataRequestDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
