using CoreGraphics;
using UIKit;

namespace DCCPanelController.View.Helpers;

public static class SetDragPreviewHelper {
    public static void SetDragPreview(object? sender, DragStartingEventArgs e, string imageName) {
        if (string.IsNullOrEmpty(imageName)) return;

        #if IOS || MACCATALYST
        e.PlatformArgs?.SetPreviewProvider(() => {
            try {
                if ((sender as VisualElement)?.Handler?.PlatformView is not UIView sourceView || sourceView.Window is null) {
                    Console.WriteLine("Error setting drag preview: No source view found.");
                    return null;
                }

                var image = UIImage.FromBundle(Path.GetFileNameWithoutExtension(imageName))
                         ?? UIImage.FromFile(imageName);
                if (image is null) {
                    Console.WriteLine("Error setting drag preview: Image not found.");
                    return null;
                }

                var iv = new UIImageView(image) {
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    Frame = new CoreGraphics.CGRect(0, 0, 32, 32)
                };

                var target = new UIDragPreview(iv);
            } catch (Exception ex) {
                Console.WriteLine($"Error setting drag preview: {ex.Message}");
                return null;
            }
            return null;
        });
        #endif
    }
}