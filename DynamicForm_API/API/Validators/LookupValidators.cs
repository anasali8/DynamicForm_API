using DynamicForm_API.API.DTOs.Lookups;
using FluentValidation;

namespace DynamicForm_API.API.Validators
{
    public class CreateLookupTableRequestDtoValidator : AbstractValidator<CreateLookupTableRequestDto>
    {
        public CreateLookupTableRequestDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(100).Matches("^[A-Za-z0-9_-]+$");
            RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    public class UpdateLookupTableRequestDtoValidator : AbstractValidator<UpdateLookupTableRequestDto>
    {
        public UpdateLookupTableRequestDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    public class ArchiveLookupTableRequestDtoValidator : AbstractValidator<ArchiveLookupTableRequestDto>
    {
        public ArchiveLookupTableRequestDtoValidator()
        {
            RuleFor(x => x.Reason).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }

    public class CreateLookupItemRequestDtoValidator : AbstractValidator<CreateLookupItemRequestDto>
    {
        public CreateLookupItemRequestDtoValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Order).GreaterThan(0);
            RuleFor(x => x.ParentValue).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.ParentValue));
        }
    }

    public class UpdateLookupItemRequestDtoValidator : AbstractValidator<UpdateLookupItemRequestDto>
    {
        public UpdateLookupItemRequestDtoValidator()
        {
            RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Order).GreaterThan(0);
            RuleFor(x => x.ParentValue).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.ParentValue));
        }
    }

    public class ReorderLookupItemDtoValidator : AbstractValidator<ReorderLookupItemDto>
    {
        public ReorderLookupItemDtoValidator()
        {
            RuleFor(x => x.LookupItemId).GreaterThan(0);
            RuleFor(x => x.Order).GreaterThan(0);
        }
    }

    public class ReorderLookupItemsRequestDtoValidator : AbstractValidator<ReorderLookupItemsRequestDto>
    {
        public ReorderLookupItemsRequestDtoValidator()
        {
            RuleFor(x => x.Items).NotEmpty();
            RuleForEach(x => x.Items).SetValidator(new ReorderLookupItemDtoValidator());
            RuleFor(x => x.Items)
                .Must(items => items.Select(i => i.LookupItemId).Distinct().Count() == items.Count)
                .WithMessage("Lookup item ids in reorder request must be unique.");
            RuleFor(x => x.Items)
                .Must(items => items.Select(i => i.Order).Distinct().Count() == items.Count)
                .WithMessage("Order values in reorder request must be unique.");
        }
    }
}
