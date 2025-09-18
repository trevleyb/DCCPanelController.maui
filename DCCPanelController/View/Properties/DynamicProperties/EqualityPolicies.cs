using System.Collections;
using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.View.Properties.DynamicProperties;

public interface IEqualityPolicy {
    bool AreEqual(object? a, object? b, Type t);
}

public class DefaultEqualityPolicy : IEqualityPolicy {
    public bool AreEqual(object? a, object? b, Type t) => DefaultEquality.AreEqual(a, b, t);
}

public static class DefaultEquality {
    public static bool AreEqual(object? a, object? b, Type? t = null) {
        if (a is null || b is null) return a is null && b is null;
        if (!(a is IEnumerable) && ReferenceEquals(a, b)) return true;
        
        if (t is { } && typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string)) {
            // Special-case TurnoutActions for structural equality
            if (a is TurnoutActions ta && b is TurnoutActions tb) {
                if (ta.Count != tb.Count) return false;
                for (var i = 0; i < ta.Count; i++) {
                    var x = ta[i]; var y = tb[i];
                    if (!string.Equals(x.ActionID, y.ActionID, StringComparison.Ordinal)) return false;
                    if (x.WhenClosed != y.WhenClosed) return false;
                    if (x.WhenThrown != y.WhenThrown) return false;
                }
                return true;
            }

            if (a is ButtonActions ba && b is ButtonActions bb) {
                if (ba.Count != bb.Count) return false;
                for (var i = 0; i < ba.Count; i++) {
                    var x = ba[i]; var y = bb[i];
                    if (!string.Equals(x.ActionID, y.ActionID, StringComparison.Ordinal)) return false;
                    if (x.WhenOff != y.WhenOff) return false;
                    if (x.WhenOn != y.WhenOn) return false;
                }
                return true;
            }
            
            // generic fallback: element-wise Equals
            var ea = ((IEnumerable)a).Cast<object?>();
            var eb = ((IEnumerable)b).Cast<object?>();
            return ea.SequenceEqual(eb);
        }

        
        if (a is float f1 && b is float f2) return Math.Abs(f1 - f2) < 1e-4f;
        if (a is double d1 && b is double d2) return Math.Abs(d1 - d2) < 1e-7;
        if (a is string s1 && b is string s2) return string.Equals(s1, s2, StringComparison.Ordinal);

        if (t is { }) {
            if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string)) {
                var ea = ((IEnumerable)a).Cast<object?>();
                var eb = ((IEnumerable)b).Cast<object?>();
                return ea.SequenceEqual(eb);
            }
        }
        if (ReferenceEquals(a,b)) return true;
        return a.Equals(b);
    }
}