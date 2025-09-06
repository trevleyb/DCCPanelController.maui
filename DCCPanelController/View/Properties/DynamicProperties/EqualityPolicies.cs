namespace DCCPanelController.View.Properties.DynamicProperties;

public interface IEqualityPolicy {
    bool AreEqual(object? a, object? b, Type t);
}

public class DefaultEqualityPolicy : IEqualityPolicy {
    public bool AreEqual(object? a, object? b, Type t) => DefaultEquality.AreEqual(a, b, t);
}

public static class DefaultEquality {
    public static bool AreEqual(object? a, object? b, Type? t = null) {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;

        if (a is float f1 && b is float f2) return Math.Abs(f1 - f2) < 1e-4f;
        if (a is double d1 && b is double d2) return Math.Abs(d1 - d2) < 1e-7;
        if (a is string s1 && b is string s2) return string.Equals(s1, s2, StringComparison.Ordinal);

        if (t is not null) {
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t) && t != typeof(string)) {
                var ea = ((System.Collections.IEnumerable)a).Cast<object?>();
                var eb = ((System.Collections.IEnumerable)b).Cast<object?>();
                return ea.SequenceEqual(eb);
            }
        }
        return a.Equals(b);
    }
}