using DynamicForm_API.API.DTOs.Fields;
using FluentValidation;
using System.Text.Json;

namespace DynamicForm_API.API.Validators
{
    public class AddDraftFieldRequestDtoValidator : AbstractValidator<AddDraftFieldRequestDto>
    {
        public AddDraftFieldRequestDtoValidator()
        {
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100).Matches("^[A-Za-z0-9_]+$");
            RuleFor(x => x.Order).GreaterThan(0);
            RuleFor(x => x.RegexPattern).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.RegexPattern));
            RuleFor(x => x.ConfigJson)
                .Must(ValidationHelpers.BeValidJson)
                .When(x => !string.IsNullOrWhiteSpace(x.ConfigJson))
                .WithMessage("ConfigJson must be a valid JSON string.");
        }
    }

    public class UpdateDraftFieldRequestDtoValidator : AbstractValidator<UpdateDraftFieldRequestDto>
    {
        public UpdateDraftFieldRequestDtoValidator()
        {
            RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Order).GreaterThan(0);
            RuleFor(x => x.RegexPattern).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.RegexPattern));
            RuleFor(x => x.ConfigJson)
                .Must(ValidationHelpers.BeValidJson)
                .When(x => !string.IsNullOrWhiteSpace(x.ConfigJson))
                .WithMessage("ConfigJson must be a valid JSON string.");
        }
    }

    public class ReorderDraftFieldItemDtoValidator : AbstractValidator<ReorderDraftFieldItemDto>
    {
        public ReorderDraftFieldItemDtoValidator()
        {
            RuleFor(x => x.FieldId).GreaterThan(0);
            RuleFor(x => x.Order).GreaterThan(0);
        }
    }

    public class ReorderDraftFieldsRequestDtoValidator : AbstractValidator<ReorderDraftFieldsRequestDto>
    {
        public ReorderDraftFieldsRequestDtoValidator()
        {
            RuleFor(x => x.Items).NotEmpty();
            RuleForEach(x => x.Items).SetValidator(new ReorderDraftFieldItemDtoValidator());
            RuleFor(x => x.Items)
                .Must(items => items.Select(i => i.FieldId).Distinct().Count() == items.Count)
                .WithMessage("Field ids in reorder request must be unique.");
            RuleFor(x => x.Items)
                .Must(items => items.Select(i => i.Order).Distinct().Count() == items.Count)
                .WithMessage("Order values in reorder request must be unique.");
        }
    }

    internal static class ValidationHelpers
    {
        public static bool BeValidJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
