namespace DCCPanelController.View.Properties.DynamicProperties;

public enum Severity { Info, Warning, Error }

public sealed record ValidationIssue(string Code, string Message, Severity Severity) {
    public static ValidationIssue Error(string code, string message) => new(code, message, Severity.Error);
    public static ValidationIssue Warn(string code, string message) => new(code, message, Severity.Warning);
    public static ValidationIssue InfoMsg(string code, string message) => new(code, message, Severity.Info);
}

public sealed class ValidationSummary {
    public IReadOnlyList<ValidationIssue> Issues { get; }
    public bool HasErrors => Issues.Any(i => i.Severity == Severity.Error);
    public ValidationSummary(IEnumerable<ValidationIssue> issues) => Issues = issues.ToList();
}

public interface IValidationRule {
    bool AppliesTo(PropertyRow row);
    Task<ValidationIssue?> EvaluateAsync(FormContext ctx, PropertyRow row);
}

public interface IValidator {
    Task<ValidationSummary> ValidateAsync(FormContext ctx, IEnumerable<PropertyRow> rows);
}

public sealed class CompositeValidator : IValidator {
    private readonly IReadOnlyList<IValidationRule> _rules;
    public CompositeValidator(IEnumerable<IValidationRule> rules) => _rules = rules.ToList();

    public async Task<ValidationSummary> ValidateAsync(FormContext ctx, IEnumerable<PropertyRow> rows) {
        var issues = new List<ValidationIssue>();
        foreach (var row in rows) {
            row.Issues.Clear();
            foreach (var rule in _rules) {
                if (!rule.AppliesTo(row)) continue;
                var issue = await rule.EvaluateAsync(ctx, row).ConfigureAwait(false);
                if (issue != null) {
                    row.Issues.Add(issue);
                    issues.Add(issue);
                }
            }
        }
        return new ValidationSummary(issues);
    }
}