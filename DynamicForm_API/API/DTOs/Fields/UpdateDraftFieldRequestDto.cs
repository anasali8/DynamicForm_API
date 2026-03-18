namespace DynamicForm_API.API.DTOs.Fields
{
    public class UpdateDraftFieldRequestDto
    {
        public string Label { get; set; } = null!;
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public string? GroupKey { get; set; }
        public string? BindingKey { get; set; }
        public string? RegexPattern { get; set; }
        public string? ConfigJson { get; set; }
    }
}
