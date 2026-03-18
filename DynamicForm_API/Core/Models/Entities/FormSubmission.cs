using DynamicForm_API.Core.Models.Shared;

namespace DynamicForm_API.Core.Models.Entities
{
    public class FormSubmission : BaseEntity
    {
        public int FormId { get; set; }
        public int FormVersionId { get; set; }

        public string UserId { get; set; } = null!;
        public string SubmissionData { get; set; } = null!;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public Form Form { get; set; } = null!;
        public FormVersion FormVersion { get; set; } = null!;
    }
}
