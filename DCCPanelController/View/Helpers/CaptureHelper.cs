using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Maui.Dispatching;

namespace DCCPanelController.View.Helpers;

public static class CaptureHelper {
    private static readonly ConditionalWeakTable<VisualElement, SemaphoreSlim> _locks = new();

    public static async Task<IScreenshotResult?> CaptureStableAsync(VisualElement view,
        int settleMs = 16,
        bool fallbackToWindow = true) {
        var gate = _locks.GetValue(view, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try {
            return await MainThread.InvokeOnMainThreadAsync(async () => {
                // 1) Wait until realized/loaded
                if (!view.IsLoaded) await WaitForLoadedAsync(view);
                await WaitForHandlerAsync(view); // ensure a platform view exists

                // 2) Optional: ensure there is a Page ancestor (good proxy for being in the tree)
                if (FindParentPage(view) is null) {
                    // It might still work, but likely not in the visual tree yet
                    await NextUiIdleAsync();
                }

                // 3) Let layout/compositor settle
                await NextUiIdleAsync();
                await Task.Delay(settleMs);

                // 4) Must have size and be visible
                if (view.Width <= 0 || view.Height <= 0) return await TryFallback(fallbackToWindow);
                if (!view.IsVisible || view.Opacity <= 0) return await TryFallback(fallbackToWindow);

                // 5) Capture
                try {
                    return await view.CaptureAsync();
                } catch {
                    return await TryFallback(fallbackToWindow);
                }

                async Task<IScreenshotResult?> TryFallback(bool allow) {
                    if (!allow) return null;
                    try {
                        return await Screenshot.Default.CaptureAsync();
                    } catch {
                        return null;
                    }
                }
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

    private static Task WaitForHandlerAsync(VisualElement view) {
        if (view.Handler is not null) return Task.CompletedTask;
        var tcs = new TaskCompletionSource();

        void HandlerChanged(object? s, EventArgs e) {
            if (view.Handler is not null) {
                view.HandlerChanged -= HandlerChanged;
                tcs.TrySetResult();
            }
        }

        view.HandlerChanged += HandlerChanged;
        return tcs.Task;
    }

    private static Page? FindParentPage(Element? e) {
        while (e is not null) {
            if (e is Page p) return p;
            e = e.Parent;
        }
        return null;
    }

    private static async Task NextUiIdleAsync() {
        var tcs = new TaskCompletionSource();
        Application.Current?.Dispatcher?.Dispatch(() => tcs.TrySetResult());
        await tcs.Task;
        await Task.Yield();
    }
}

