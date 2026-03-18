using DynamicForm_API.Core.Models.Shared;
using DynamicForm_API.Shared.Enums;
using Microsoft.AspNetCore.Http;

namespace DynamicForm_API.Core.Models.Entities
{
    public class FormVersion : AuditTableEntity
    {
        public int FormId { get; set; }
        public int VersionNumber { get; set; }
        public FormVersionStatus Status { get; set; } = FormVersionStatus.Draft;
        public bool IsCurrent { get; set; } = false;
        public DateTime? PublishedAt { get; set; }

        public Form Form { get; set; } = null!;
        public ICollection<FormField> Fields { get; set; } = new List<FormField>();
        public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
    }
}
