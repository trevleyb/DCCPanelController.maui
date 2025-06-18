using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using DCCPanelController.Helpers;

namespace DCCPanelController.Models.DataModel.Helpers;

public class PanelChangeDetector {
    private string? _lastChecksum;
    private readonly PanelChangeDetectorOptions _options;
    private readonly Dictionary<object, int> _objectDepths = new();

    public PanelChangeDetector(Panel? panel, PanelChangeDetectorOptions? options = null) {
        _options = options ?? new PanelChangeDetectorOptions();
        ResetChecksum(panel);
    }

    public void ResetChecksum(Panel? panel) {
        _lastChecksum = (panel is null) ? string.Empty : GeneratePanelChecksum(panel);
    }
    
    public bool HasPanelChanged(Panel? panel) {
        if (panel is not null) {
            using (new CodeTimer("PanelChangeDetector.HasPanelChanged", false)) {
                var currentChecksum = GeneratePanelChecksum(panel);
                var hasChanged = _lastChecksum != null && _lastChecksum != currentChecksum;
                return hasChanged;
            }
        }
        return false;
    }

    private string GeneratePanelChecksum(Panel panel) {
        var sb = new StringBuilder();
        _objectDepths.Clear();
        AppendObjectProperties(sb, panel, "Panel");
        return ComputeHash(sb.ToString());
    }

    private void AppendObjectProperties(StringBuilder sb, object? obj, string prefix = "", int depth = 0) {
        if (obj == null) {
            sb.Append($"{prefix}:null|");
            return;
        }

        // Check depth limit
        if (depth >= _options.MaxDepth) {
            sb.Append($"{prefix}:max_depth_reached|");
            return;
        }

        // Check for circular references
        if (_objectDepths.TryGetValue(obj, out var existingDepth) && existingDepth < depth) {
            sb.Append($"{prefix}:circular_ref|");
            return;
        }

        _objectDepths[obj] = depth;

        var type = obj.GetType();

        // Handle simple types
        if (IsSimpleType(type)) {
            sb.Append($"{prefix}:{obj}|");
            return;
        }

        // Handle collections
        if (obj is IEnumerable enumerable && type != typeof(string)) {
            var items = enumerable.Cast<object>().ToList();
            sb.Append($"{prefix}_Count:{items.Count}|");

            for (var i = 0; i < items.Count; i++) {
                AppendObjectProperties(sb, items[i], $"{prefix}[{i}]", depth + 1);
            }
            return;
        }

        // Get properties based on options
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        if (_options.IncludePrivateProperties)
            bindingFlags |= BindingFlags.NonPublic;

        var properties = type.GetProperties(bindingFlags)
                             .Where(p => p is { CanRead: true, CanWrite: true } && p.GetIndexParameters().Length == 0)
                             .Where(p => !ShouldSkipProperty(p))
                             .OrderBy(p => p.Name);

        foreach (var property in properties) {
            try {
                var value = property.GetValue(obj);
                var propertyPath = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

                AppendObjectProperties(sb, value, propertyPath, depth + 1);
            } catch (Exception ex) {
                sb.Append($"{prefix}.{property.Name}:error_{ex.GetType().Name}|");
            }
        }
    }

    private bool ShouldSkipProperty(PropertyInfo property) {
        // Check skip list
        if (_options.SkipProperties.Any(skip => property.Name.Contains(skip)))
            return true;

        // Check skip types
        if (_options.SkipTypes.Any(skipType => skipType.IsAssignableFrom(property.PropertyType)))
            return true;

        // Check attributes
        if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
            return true;

        return false;
    }

    private bool IsSimpleType(Type type) {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(decimal) ||
               type == typeof(Guid) ||
               type == typeof(Uri) ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                IsSimpleType(type.GetGenericArguments()[0]));
    }

    private string ComputeHash(string input) {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}