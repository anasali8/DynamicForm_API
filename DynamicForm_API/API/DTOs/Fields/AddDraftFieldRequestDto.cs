using DynamicForm_API.Shared.Enums;

namespace DynamicForm_API.API.DTOs.Fields
{
    public class AddDraftFieldRequestDto
    {
        public FieldType Type { get; set; }
        public string Label { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public string? GroupKey { get; set; }
        public string? BindingKey { get; set; }
        public string? RegexPattern { get; set; }
        public string? ConfigJson { get; set; }
    }
}
