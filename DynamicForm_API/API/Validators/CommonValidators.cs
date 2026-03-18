using DynamicForm_API.API.DTOs.Common;
using FluentValidation;

namespace DynamicForm_API.API.Validators
{
    public class PagedRequestDtoValidator : AbstractValidator<PagedRequestDto>
    {
        public PagedRequestDtoValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, PagedRequestDto.MaxPageSize);
        }
    }
}
