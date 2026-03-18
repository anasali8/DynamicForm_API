using DynamicForm_API.API.DTOs.Submissions;
using FluentValidation;

namespace DynamicForm_API.API.Validators
{
    public class SubmitFormVersionRequestDtoValidator : AbstractValidator<SubmitFormVersionRequestDto>
    {
        public SubmitFormVersionRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.SubmissionData)
                .NotNull()
                .Must(data => data.Count > 0)
                .WithMessage("SubmissionData must contain at least one key.");

            RuleForEach(x => x.SubmissionData.Keys)
                .NotEmpty()
                .MaximumLength(100)
                .Matches("^[A-Za-z0-9_]+$");
        }
    }
}
