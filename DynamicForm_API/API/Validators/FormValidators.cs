using DynamicForm_API.API.DTOs.Forms;
using FluentValidation;

namespace DynamicForm_API.API.Validators
{
    public class UpdateFormMetadataRequestDtoValidator : AbstractValidator<UpdateFormMetadataRequestDto>
    {
        public UpdateFormMetadataRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    public class ArchiveFormRequestDtoValidator : AbstractValidator<ArchiveFormRequestDto>
    {
        public ArchiveFormRequestDtoValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
