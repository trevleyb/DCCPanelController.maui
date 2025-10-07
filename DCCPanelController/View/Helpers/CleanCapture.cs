using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Media; // ScreenshotFormat

namespace DCCPanelController.View.Helpers;

public static class CleanCapture {
    private static readonly ConditionalWeakTable<VisualElement, SemaphoreSlim> _locks = new();

    public static async Task<IScreenshotResult?> CaptureAsync(VisualElement view, int settleMs = 16) {
        var gate = _locks.GetValue(view, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try {
            return await MainThread.InvokeOnMainThreadAsync(async () => {
                if (!view.IsLoaded)
                    await WaitForLoadedAsync(view);

                await NextUiIdleAsync();
                await Task.Delay(settleMs);

                if (view.Width <= 0 || view.Height <= 0 || !view.IsVisible || view.Opacity <= 0)
                    return null;

                #if IOS || MACCATALYST
                var uiView = view.Handler?.PlatformView as UIKit.UIView;
                if (uiView is null) return null;

                // Round to whole pixels to avoid half-pixel slivers
                var rawBounds = uiView.Bounds;
                var x = Math.Floor(rawBounds.X);
                var y = Math.Floor(rawBounds.Y);
                var w = Math.Ceiling(rawBounds.Width);
                var h = Math.Ceiling(rawBounds.Height);
                var bounds = new CoreGraphics.CGRect(x, y, w, h);

                // Device scale
                var deviceScale = UIKit.UIScreen.MainScreen.Scale;

                // Renderer: allow transparency, we’ll paint bg ourselves
                var fmt = new UIKit.UIGraphicsImageRendererFormat {
                    Opaque = false,
                    Scale = deviceScale
                };

                using var renderer = new UIKit.UIGraphicsImageRenderer(bounds.Size, fmt);

                UIKit.UIColor EffectiveBg() {
                    var cur = uiView;
                    while (cur != null) {
                        if (cur.BackgroundColor != null) return cur.BackgroundColor!;
                        cur = cur.Superview;
                    }
                    return UIKit.UIColor.SystemBackground;
                }

                var uiImage = renderer.CreateImage(ctx => {
                    // Fill background explicitly
                    var cg = ctx.CGContext;
                    EffectiveBg().SetFill();
                    cg.FillRect(new CoreGraphics.CGRect(0, 0, bounds.Width, bounds.Height));

                    // Render view
                    uiView.DrawViewHierarchy(bounds, true);
                });

                // pixel metadata
                var pixelWidth = (int)Math.Round(bounds.Width * uiImage.CurrentScale);
                var pixelHeight = (int)Math.Round(bounds.Height * uiImage.CurrentScale);
                var density = (double)uiImage.CurrentScale;

                return new UiImageScreenshotResult(uiImage, pixelWidth, pixelHeight, density);
                #else
                // Other platforms: use MAUI’s built-in capture
                return await view.CaptureAsync();
                #endif
            });
        } finally {
            gate.Release();
        }
    }

    private static Task WaitForLoadedAsync(VisualElement view) {
        if (view.IsLoaded) return Task.CompletedTask;
        var tcs = new TaskCompletionSource();

        void Handler(object? s, EventArgs e) {
            view.Loaded -= Handler;
            tcs.TrySetResult();
        }

        view.Loaded += Handler;
        return tcs.Task;
    }

    private static async Task NextUiIdleAsync() {
        var tcs = new TaskCompletionSource();
        Application.Current?.Dispatcher?.Dispatch(() => tcs.TrySetResult());
        await tcs.Task;
        await Task.Yield();
    }

    #if IOS || MACCATALYST
    /// <summary>
    /// IScreenshotResult backed by a UIImage. Encodes PNG/JPEG on demand.
    /// </summary>
    private sealed class UiImageScreenshotResult : IScreenshotResult {
        private readonly UIKit.UIImage _image;

        public UiImageScreenshotResult(UIKit.UIImage image, int width, int height, double density) {
            _image = image ?? throw new ArgumentNullException(nameof(image));
            Width = width;
            Height = height;
            Density = density;
        }

        public int Width { get; }
        public int Height { get; }
        public double Density { get; }

        public Task<Stream> OpenReadAsync() => OpenReadAsync(ScreenshotFormat.Png, quality: 100);

        public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100) {
            return Task.FromResult<Stream>(EncodeToStream(format, quality));
        }

        public async Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100) {
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            using var s = EncodeToStream(format, quality);
            await s.CopyToAsync(destination).ConfigureAwait(false);
        }

        private Stream EncodeToStream(ScreenshotFormat format, int quality) {
            Foundation.NSData? data = null;
            switch (format) {
                case ScreenshotFormat.Png:
                    data = _image.AsPNG();
                break;

                case ScreenshotFormat.Jpeg:
                    var q = (NFloat)Math.Clamp(quality / 100f, 0f, 1f);
                    data = _image.AsJPEG(q);
                break;

                default:
                    // Fallback to PNG if an unknown format is passed
                    data = _image.AsPNG();
                break;
            }
            if (data is null) throw new InvalidOperationException("Failed to encode screenshot.");
            var bytes = data.ToArray();

            // MemoryStream not writable; caller reads and disposes
            return new MemoryStream(bytes, writable: false);
        }

        // Let GC release the UIImage; if you want deterministic disposal, implement IDisposable and call _image.Dispose()
    }
    #endif
}