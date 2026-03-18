namespace DynamicForm_API.API.DTOs.Fields
{
    public class ReorderDraftFieldsRequestDto
    {
        public IReadOnlyList<ReorderDraftFieldItemDto> Items { get; set; } = Array.Empty<ReorderDraftFieldItemDto>();
    }
}
