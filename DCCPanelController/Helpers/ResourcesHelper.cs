using System.Reflection;

namespace DCCPanelController.Helpers;

public static class ResourcesHelper
{
    private static IEnumerable<string> GetImageResourceNames(Assembly assembly, string resourceFolder) {
        // Retrieve all resources from the assembly
        var resourceNames = assembly.GetManifestResourceNames();
 
        // Filter resources to get only those in the specified folder
        var imageResourceNames = resourceNames
            .Where(name => name.StartsWith(resourceFolder, StringComparison.OrdinalIgnoreCase))
            .Where(name => name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                           name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        return imageResourceNames;
    }

    private static ImageSource GetImageSource(string resourceName, Assembly assembly) {
        return ImageSource.FromResource(resourceName, assembly);
    }

    public static IEnumerable<ImageSource> GetSymbolImages() {
        var resourceFolder = "DCCPanelController.Resources.Symbols"; 
        var assembly = Assembly.GetExecutingAssembly();
        var imageNames = ResourcesHelper.GetImageResourceNames(assembly, resourceFolder);

        foreach (var imageName in imageNames) {
            var imageSource = GetImageSource(imageName, assembly);
            yield return imageSource;
        }
    }
}