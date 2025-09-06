using System.Text.RegularExpressions;

namespace DCCPanelController.View.Properties.DynamicProperties;

public class PropertyRendererRules {
    // ---- Validation Rules (sample) ----
    public sealed class RequiredRule : IValidationRule {
        public bool AppliesTo(PropertyRow row) => row.Field.Meta.GetParameters("required", false);

        public Task<ValidationIssue?> EvaluateAsync(DynamicTilePropertyForm ctx, PropertyRow row) {
            if (row.HasMixedValues && !row.IsTouched) return System.Threading.Tasks.Task.FromResult<ValidationIssue?>(null);
            var v = row.CurrentValue;
            if (v is null) return System.Threading.Tasks.Task.FromResult<ValidationIssue?>(ValidationIssue.Error("required", $"{row.Field.Meta.Label} is required."));
            if (v is string s && string.IsNullOrWhiteSpace(s)) return System.Threading.Tasks.Task.FromResult<ValidationIssue?>(ValidationIssue.Error("required", $"{row.Field.Meta.Label} is required."));
            return Task.FromResult<ValidationIssue?>(null);
        }
    }

    public sealed class RangeRule : IValidationRule {
        public bool AppliesTo(PropertyRow row) => row.Field.Meta.Parameters.ContainsKey("min") || row.Field.Meta.Parameters.ContainsKey("max");

        public Task<ValidationIssue?> EvaluateAsync(DynamicTilePropertyForm ctx, PropertyRow row) {
            if (row.CurrentValue is IComparable c) {
                var hasMin = row.Field.Meta.Parameters.TryGetValue("min", out var minObj);
                var hasMax = row.Field.Meta.Parameters.TryGetValue("max", out var maxObj);
                if (hasMin && c.CompareTo(Convert.ChangeType(minObj, row.Field.Accessor.PropertyType)) < 0) return System.Threading.Tasks.Task.FromResult<ValidationIssue?>(ValidationIssue.Error("min", $"{row.Field.Meta.Label} must be ≥ {minObj}."));
                if (hasMax && c.CompareTo(Convert.ChangeType(maxObj, row.Field.Accessor.PropertyType)) > 0) return System.Threading.Tasks.Task.FromResult<ValidationIssue?>(ValidationIssue.Error("max", $"{row.Field.Meta.Label} must be ≤ {maxObj}."));
            }
            return Task.FromResult<ValidationIssue?>(null);
        }
    }

    public sealed class RegexRule : IValidationRule {
        public bool AppliesTo(PropertyRow row) => row.Field.Meta.Parameters.ContainsKey("regex");

        public Task<ValidationIssue?> EvaluateAsync(DynamicTilePropertyForm ctx, PropertyRow row) {
            if (row.CurrentValue is string s) {
                var pattern = row.Field.Meta.GetParameters<string>("regex", "");
                if (!string.IsNullOrEmpty(pattern) && !Regex.IsMatch(s, pattern)) return System.Threading.Tasks.Task.FromResult<ValidationIssue?>(ValidationIssue.Error("regex", $"{row.Field.Meta.Label} is not in the correct format."));
            }
            return Task.FromResult<ValidationIssue?>(null);
        }
    }
    
}