using DynamicForm_API.API.DTOs.Forms;
using FluentValidation;

namespace DynamicForm_API.API.Validators
{
    public class CreateFormDraftRequestDtoValidator : AbstractValidator<CreateFormDraftRequestDto>
    {
        public CreateFormDraftRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(100)
                .Matches("^[A-Za-z0-9_-]+$");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
