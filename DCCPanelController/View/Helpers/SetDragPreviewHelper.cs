
using System.Diagnostics;
#if IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif

namespace DCCPanelController.View.Helpers;

public static class SetDragPreviewHelper {
    public static void SetDragPreview(object? sender, DragStartingEventArgs e, string imageName) {
        if (string.IsNullOrEmpty(imageName)) return;

        #if IOS || MACCATALYST
        e.PlatformArgs?.SetPreviewProvider(() => {
            try {
                if ((sender as VisualElement)?.Handler?.PlatformView is not UIView sourceView || sourceView.Window is null) {
                    return null;
                }

                var image = UIImage.FromBundle(Path.GetFileNameWithoutExtension(imageName)) ?? UIImage.FromFile(imageName);
                if (image is null) return null;

                var iv = new UIImageView(image) {
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    Frame = new CGRect(0, 0, 32, 32),
                };

                var target = new UIDragPreview(iv);
            } catch {
                return null;
            }
            return null;
        });
        #endif
    }
}