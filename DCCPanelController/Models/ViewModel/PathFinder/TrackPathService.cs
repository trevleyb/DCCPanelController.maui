using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.Models.ViewModel.PathFinder;

public class PathTracingService {
    private readonly TrackPathTracer _tracer = new();
    private CancellationTokenSource? _currentTracingCts;

    public void RegisterTile(TrackTile tile) {
        if (tile.Entity is TrackEntity entity) _tracer.RegisterTile(entity, tile);
    }

    public void UnregisterTile(TrackTile tile) {
        if (tile.Entity is TrackEntity entity) _tracer.UnregisterTile(entity);
    }

    public void ClearTileRegistry() {
        _tracer.ClearTileRegistry();
    }

    public async Task StartPathTracing(TrackTile tile) {
        if (tile.Entity is TrackEntity entity) await StartPathTracing(entity);
    }

    public async Task StartPathTracing(TrackEntity startTrack) {
        try {
            await StopPathTracing();
            _currentTracingCts = new CancellationTokenSource();
            try {
                var paths = await _tracer.TracePathsFromTrackAsync(startTrack, _currentTracingCts.Token);
                var animator = new TrackPathAnimator();
                await animator.AnimatePathsAsync(paths, _tracer.RegisteredTiles(), _currentTracingCts.Token);
            } catch (OperationCanceledException) {
                // Do nothing
            } finally {
                _currentTracingCts?.Dispose();
                _currentTracingCts = null;
            }
        } catch (Exception ) {
            // Do Nothing
        }
    }

    private async Task StopPathTracing() {
        if (_currentTracingCts is not null) {
            _currentTracingCts?.CancelAsync();
            _currentTracingCts?.Dispose();
            _currentTracingCts = null;
            await Task.Delay(100);
        }
    }
}