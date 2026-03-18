namespace DynamicForm_API.Core.Models.Shared
{
    public class AuditTableEntity : BaseEntity
    { 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
