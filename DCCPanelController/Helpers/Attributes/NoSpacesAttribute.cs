using System.ComponentModel.DataAnnotations;

namespace DCCPanelController.Helpers.Attributes;

public sealed class NoSpacesAttribute : ValidationAttribute
{
    public NoSpacesAttribute() { }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var str = value as string;
        if (str != null) {
            var num = str.IndexOf(" ", StringComparison.Ordinal);
            if (num == -1) {
                return ValidationResult.Success;
            }
            return new ValidationResult("The current value includes space");
        }
        return ValidationResult.Success;
    }
}