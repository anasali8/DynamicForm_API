using DynamicForm_API.API.DTOs.Versions;
using FluentValidation;

namespace DynamicForm_API.API.Validators
{
    public class CreateNextDraftVersionRequestDtoValidator : AbstractValidator<CreateNextDraftVersionRequestDto>
    {
        public CreateNextDraftVersionRequestDtoValidator()
        {
            RuleFor(x => x.SourceVersionId)
                .GreaterThan(0)
                .When(x => x.SourceVersionId.HasValue);
        }
    }

    public class UpdateDraftVersionRequestDtoValidator : AbstractValidator<UpdateDraftVersionRequestDto>
    {
        public UpdateDraftVersionRequestDtoValidator()
        {
            RuleFor(x => x.ChangeNote)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.ChangeNote));
        }
    }

    public class PublishDraftVersionRequestDtoValidator : AbstractValidator<PublishDraftVersionRequestDto>
    {
        public PublishDraftVersionRequestDtoValidator()
        {
            RuleFor(x => x.PublishNote)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.PublishNote));
        }
    }

    public class ArchiveVersionRequestDtoValidator : AbstractValidator<ArchiveVersionRequestDto>
    {
        public ArchiveVersionRequestDtoValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
