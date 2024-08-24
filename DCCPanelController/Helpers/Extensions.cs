using System.Xml.Linq;

namespace DCCPanelController.Helpers;

public static class Extensions {

    public static char GetSortDirection(this bool isAscending) {
        return isAscending ? '▼' : '▲';
    }
    
    public static bool IsTypeOf(this XElement element, string type) => element.Name.LocalName.Equals(type, StringComparison.OrdinalIgnoreCase);

    public static string ToString(this bool value) => value ? "True" : "False";
    
    public static bool IsFalse(this string value) => !IsTrue(value);
    public static bool IsTrue(this string value) {
        if (string.IsNullOrEmpty(value)) return false;
        return value.ToLowerInvariant() switch {
            "true"  => true,
            "false" => false,
            "0"     => false,
            "1"     => true,
            "t"     => true,
            "f"     => false,
            "ok"    => true,
            "on"    => true,
            "off"   => false,
            _       => false
        };
    }
}