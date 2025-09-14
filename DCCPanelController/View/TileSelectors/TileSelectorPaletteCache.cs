// TileSelectorPaletteCache.cs

using System.Collections.Concurrent;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View.TileSelectors;

public class TileSelectorPaletteCache {
    // Use a unique key for each unique Panel/Palette
    private static readonly ConcurrentDictionary<string, PaletteResult> Cache = new();

    public static PaletteResult GetOrBuild(Panel panel) {
        var key = panel.Guid.ToString();
        if (Cache.TryGetValue(key, out var cachedResult)) return cachedResult;
        var result = BuildTilesForPanel(panel);
        if (result is null) throw new InvalidOperationException("Unable to build palette");
        Cache.TryAdd(key, result);
        return result;
    }

    public static void Prebuild(Panel panel) => GetOrBuild(panel);
    public static void Clear(string key) => Cache.TryRemove(key, out _);
    public static void Clear() => Cache.Clear();

    public static PaletteResult? BuildTilesForPanel(Panel editPanel) {
        try {
            var byCategory = new Dictionary<string, List<ITile>>();
            var categories = new List<string>();

            void Add(string category, IEnumerable<Entity> entities) {
                categories.Add(category);
                byCategory[category] = new List<ITile>();
                foreach (var e in entities) {
                    var tile = TileFactory.CreateTile(e, 32, TileDisplayMode.Symbol);
                    if (tile is { }) byCategory[category].Add(tile);
                }
            }

            var panel = editPanel.CloneEmptyPanel("Selector");
            Add("Actions", [
                new TurnoutButtonEntity(panel) { State = ButtonStateEnum.On },
                new ActionButtonEntity(panel) { State = ButtonStateEnum.On },
                new RouteEntity(panel) { State = RouteStateEnum.Active },
                new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Light, State = ButtonStateEnum.On },
            ]);

            Add("Mainline", [
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new StraightContinuationEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new CornerEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new CornerContinuationEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new LeftTurnoutEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new RightTurnoutEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new TerminatorEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new CrossingEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new TunnelEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new BridgeEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new PlatformEntity(panel) { TrackType = TrackTypeEnum.MainLine },
            ]);

            Add("Branch", [
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new StraightContinuationEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new CornerEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new CornerContinuationEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new LeftTurnoutEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new RightTurnoutEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new TerminatorEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new CrossingEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new TunnelEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new BridgeEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new PlatformEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
            ]);

            Add("Shapes", [
                new TextEntity(panel),
                new CircleLabelEntity(panel),
                new RectangleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black },
                new LineEntity(panel) { Height = 1, Width = 1, LineColor = Colors.Black, LineWidth = 4 },
                new CircleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black },
                new ImageEntity(panel),
            ]);
            return new PaletteResult(byCategory, categories);
        } catch (Exception ex) {
            Console.WriteLine($"Error building palette: {ex.Message}");
            return null;
        }
    }

    public record PaletteResult(Dictionary<string, List<ITile>> ByCategory, List<string> Categories);
}