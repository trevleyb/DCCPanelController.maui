using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DCCPanelController.Helpers;

public static class Extensions {
    public static char GetSortDirection(this bool isAscending) {
        return isAscending ? '▼' : '▲';
    }

    public static bool IsTypeOf(this XElement element, string type) {
        return element.Name.LocalName.Equals(type, StringComparison.OrdinalIgnoreCase);
    }

    public static string ToString(this bool value) {
        return value ? "True" : "False";
    }

    public static bool IsFalse(this string value) {
        return !IsTrue(value);
    }

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

    public static byte[] ToByteArray(this XDocument document) {
        using (var memoryStream = new MemoryStream()) {
            using (var writer = XmlWriter.Create(memoryStream, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = false })) {
                document.WriteTo(writer);
            }
            return memoryStream.ToArray();
        }
    }

    public static async Task<string> RenderSchematicToBase64ImageAsync(this IView view) {
        return Convert.ToBase64String(await RenderSchematicToImageStreamAsync(view) ?? []);
    }

    public static async Task<byte[]?> RenderSchematicToImageStreamAsync(this IView view) {
        try {
            var image = await view.CaptureAsync();
            if (image is null) return null;

            using (var stream = new MemoryStream()) {
                await image.CopyToAsync(stream);
                return stream.ToArray();
            }
        } catch {
            return null;
        }
    }
}