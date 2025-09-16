using DCCPanelController.Models.ViewModel.Tiles;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace DCCPanelController.Models.ViewModel.Helpers;

internal sealed class TileRenderCache : IDisposable {
    private readonly ConcurrentDictionary<TileRenderKey, (SKImage Img, int Gen)> _map = new();
    private readonly LinkedList<TileRenderKey> _lru = new();
    private readonly Dictionary<TileRenderKey, LinkedListNode<TileRenderKey>> _nodes = new();
    private readonly object _gate = new();
    private int _gen;
    private readonly int _capacity;
    public static TileRenderCache Shared { get; } = new(512); // tune per device

    private TileRenderCache(int capacity) => _capacity = Math.Max(64, capacity);
    public void Dispose() { lock (_gate) foreach (var kv in _map) kv.Value.Img.Dispose(); _map.Clear(); _nodes.Clear(); _lru.Clear(); }

    public bool TryGet(TileRenderKey key, out SKImage image) {
        if (_map.TryGetValue(key, out var entry)) {
            image = entry.Img;
            Touch(key);
            return true;
        }
        image = null!;
        return false;
    }

    public void Put(TileRenderKey key, SKImage image) {
        _map[key] = (image, ++_gen);
        lock (_gate) {
            if (_nodes.TryGetValue(key, out var n)) _lru.Remove(n);
            _nodes[key] = _lru.AddFirst(key);
            while (_lru.Count > _capacity) {
                var last = _lru.Last!.Value;
                _lru.RemoveLast();
                _nodes.Remove(last);
                if (_map.TryRemove(last, out var victim)) victim.Img.Dispose();
            }
        }
    }

    private void Touch(TileRenderKey key) {
        lock (_gate) {
            if (_nodes.TryGetValue(key, out var n)) {
                _lru.Remove(n);
                _nodes[key] = _lru.AddFirst(key);
            }
        }
    }

    public static string HashStyle(string? styleDescriptor) {
        // Anything stable describing SvgStyle (colors, visibilities, etc.)
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(styleDescriptor ?? "");
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}
