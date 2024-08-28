using System.ComponentModel.DataAnnotations;

namespace DCCPanelController.Helpers.Attributes;

public sealed class NoSpacesAttribute : ValidationAttribute
{
    public NoSpacesAttribute() { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null) {
            var str = value as string;
            if (str != null) {
                var num = str?.IndexOf(" ", StringComparison.Ordinal) ?? -1;
                return (num == -1 ? ValidationResult.Success : new ValidationResult("The current value includes space"));
            }
        }
        return ValidationResult.Success;
    }
}