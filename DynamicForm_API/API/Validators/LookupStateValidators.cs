using DynamicForm_API.API.DTOs.Lookups;
using FluentValidation;

namespace DynamicForm_API.API.Validators
{
    public class SetLookupItemActiveStateRequestDtoValidator : AbstractValidator<SetLookupItemActiveStateRequestDto>
    {
        public SetLookupItemActiveStateRequestDtoValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull();
        }
    }
}
