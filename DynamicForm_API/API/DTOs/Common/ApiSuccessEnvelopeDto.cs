namespace DynamicForm_API.API.DTOs.Common
{
    public class ApiSuccessEnvelopeDto<T>
    {
        public bool Success { get; set; } = true;
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
