using FluentValidation;
using ModularMonolith.Modules.Settings.Application.Requests;
using ModularMonolith.Modules.Settings.Domain;
using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Modules.Settings.Application.Validators;

public sealed class CreateSettingRequestValidator : AbstractValidator<CreateSettingRequest>
{
    public CreateSettingRequestValidator(ICurrentUser currentUser)
    {
        RuleFor(request => request.Key)
            .NotEmpty()
            .MaximumLength(SettingRules.MaxKeyLength)
            .Must(SettingRules.IsValidKeyFormat)
            .WithMessage("A setting key may only contain letters, digits, dots, dashes, underscores, and colons, and must start with a letter or digit.")
            .Must(key => !SettingRules.ContainsSecretFragment(key))
            .WithMessage("Secrets must not be stored in application settings.");

        RuleFor(request => request.DataType).IsInEnum();
        RuleFor(request => request.Scope).IsInEnum();

        RuleFor(request => request.Module)
            .MaximumLength(SettingRules.MaxModuleLength)
            .Must((request, module) =>
                request.Scope != SettingScope.Module || !string.IsNullOrWhiteSpace(module))
            .WithMessage("A module name is required for module-scoped settings.");

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

        RuleFor(request => request.AllowedValues)
            .Must((request, values) => values is null || values.All(value =>
                SettingValueValidator.IsValidValue(request.DataType, value)))
            .WithMessage("Every allowed value must match the selected data type.");

        RuleFor(request => request)
            .Custom((request, context) =>
            {
                if (request.Scope is SettingScope.Global or SettingScope.Module)
                {
                    if (request.TenantId is not null || request.UserId is not null)
                    {
                        context.AddFailure(
                            "Scope",
                            "Global and module settings cannot specify a tenant or user owner.");
                    }
                }
                else if (request.Scope == SettingScope.Tenant)
                {
                    if (request.UserId is not null)
                    {
                        context.AddFailure("Scope", "Tenant settings cannot specify a user owner.");
                    }
                }
                else if (request.Scope == SettingScope.User)
                {
                    if (request.TenantId is not null)
                    {
                        context.AddFailure("Scope", "User settings derive their tenant from the current context.");
                    }

                    if (request.UserId is null && currentUser.UserId is null)
                    {
                        context.AddFailure(
                            "UserId",
                            "User-scoped settings require an authenticated user or an explicit user identifier.");
                    }
                }

                var allowedValuesJson = SettingValueValidator.SerializeAllowedValues(request.AllowedValues);
                var errors = SettingValueValidator.Validate(
                    request.DataType,
                    request.Value,
                    request.IsRequired,
                    allowedValuesJson,
                    request.ValidationPattern);

                foreach (var error in errors)
                {
                    foreach (var message in error.Value)
                    {
                        context.AddFailure(error.Key, message);
                    }
                }
            });
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
