using FluentValidation;
using ModularMonolith.Modules.Settings.Application.Requests;
using ModularMonolith.Modules.Settings.Domain;

namespace ModularMonolith.Modules.Settings.Application.Validators;

public sealed class UpdateSettingRequestValidator : AbstractValidator<UpdateSettingRequest>
{
    public UpdateSettingRequestValidator()
    {
        RuleFor(request => request.Value)
            .MaximumLength(SettingRules.MaxValueLength);

        RuleFor(request => request.Description)
            .MaximumLength(SettingRules.MaxDescriptionLength);

        RuleFor(request => request.Category)
            .MaximumLength(SettingRules.MaxCategoryLength);

        RuleFor(request => request.ValidationPattern)
            .MaximumLength(SettingRules.MaxValidationPatternLength)
            .Must(BeValidPattern)
            .WithMessage("The validation pattern is not a valid regular expression.");

        RuleFor(request => request.AllowedValues)
            .Must(values => values is null || values.Count <= SettingRules.MaxAllowedValues)
            .WithMessage($"No more than {SettingRules.MaxAllowedValues} allowed values may be configured.")
            .Must(values => values is null || values.All(value =>
                !string.IsNullOrWhiteSpace(value)
                && value.Length <= SettingRules.MaxValueLength))
            .WithMessage("Allowed values must be non-empty and within the maximum value length.");
    }

    private static bool BeValidPattern(string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return true;
        }

        try
        {
            _ = new System.Text.RegularExpressions.Regex(
                pattern,
                System.Text.RegularExpressions.RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1));

            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
