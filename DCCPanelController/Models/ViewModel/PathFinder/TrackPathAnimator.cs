using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.PathFinder;

public class TrackPathAnimator {
    public int PathAnimationDelayMs { get; set; } = 50;
    public int PathDisplayDurationMs { get; set; } = 3000;

    public async Task AnimatePathsAsync(List<TracedPath> paths,
                                        List<TrackTile> registeredTiles,
                                        CancellationToken cancellationToken = default) {
        try {
            await ClearAllPathHighlights(registeredTiles);
            foreach (var path in paths.TakeWhile(path => !cancellationToken.IsCancellationRequested)) {
                await AnimateSinglePathAsync(path, cancellationToken);
            }
            await Task.Delay(PathDisplayDurationMs, cancellationToken);
            await ClearAllPathHighlights(registeredTiles);
        } catch (OperationCanceledException) {
            await ClearAllPathHighlights(registeredTiles);
        }
    }

    private async Task AnimateSinglePathAsync(TracedPath path, CancellationToken cancellationToken) {
        foreach (var segment in path.Segments.TakeWhile(segment => !cancellationToken.IsCancellationRequested)) {
            if (segment.Tile != null) {
                await MainThread.InvokeOnMainThreadAsync(() => { segment.Tile.IsPath = true; });
            }
            await Task.Delay(PathAnimationDelayMs, cancellationToken);
        }
    }

    private async Task ClearAllPathHighlights(List<TrackTile> registeredTiles) {
        await MainThread.InvokeOnMainThreadAsync(() => {
            // Clear all IsPath properties from registered tiles
            foreach (var tile in registeredTiles) {
                tile.IsPath = false;
            }
        });
    }
}