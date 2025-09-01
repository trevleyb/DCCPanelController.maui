namespace DCCPanelController.Views.Helpers;

public sealed class TapArbitrator(int milliseconds = 275) {
    private readonly TimeSpan _window = TimeSpan.FromMilliseconds(milliseconds);
    private CancellationTokenSource? _singleCts;

    /// Call this from the 1-tap handler
    public async void Single(Func<Task> onSingle) {
        try {
            await _singleCts?.CancelAsync()!;
            var cts = _singleCts = new CancellationTokenSource();
            try {
                await Task.Delay(_window, cts.Token);
                if (!cts.IsCancellationRequested) await onSingle();
            } catch (TaskCanceledException) { /* expected when double-tap occurs */
            } finally {
                if (_singleCts == cts) _singleCts = null;
                cts.Dispose();
            }
        } catch {
            /* Just ignore any errors */
        }
    }

    /// Call this from the 2-tap handler
    public async void Double(Func<Task> onDouble) {
        try {
            await _singleCts?.CancelAsync()!; // prevent the pending single
            _singleCts = null;
            await onDouble();
        } catch {
            /* Just ignore any errors */
        }
    }
    
}