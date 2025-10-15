using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.View.TileSelectors;

public static class PaletteCache {
    private const string DefaultPalette = "Default";

    // Store Lazy<PaletteResult> to guarantee single-construction per key
    private static readonly ConcurrentDictionary<string, Palette> Cache = new();

    // Prebuild (or return existing) without allocating a Panel unless needed
    public static Palette PrebuildDefaultPalette() => GetPalette(DefaultPalette, CreateDefaultPanel());

    public static Palette GetDefaultPalette() => GetPalette(DefaultPalette, CreateDefaultPanel());

    public static Palette GetPalette(Panel panel) => GetPalette(panel.Guid.ToString(), panel.CloneEmptyPanel("Selector"));
    
    public static void Clear(string key) => Cache.TryRemove(key, out _);

    public static void Clear() => Cache.Clear();

    private static Palette GetPalette(string key, Panel panel) {
        var result = BuildPaletteForPanel(panel);
        return result ?? throw new InvalidOperationException("Unable to build palette.");
    }

    // --- Implementation details ----------------------------------------------

    private static Panel CreateDefaultPanel() {
        var panels = new Panels();
        return panels.CreatePanel("Palette");
    }

    private static Palette BuildPaletteForPanel(Panel panel) {
        try {
            var palette = new Palette(panel);
            
            Add("Actions", [
                new TurnoutButtonEntity(panel) { State = ButtonStateEnum.On },
                new ActionButtonEntity(panel) { State = ButtonStateEnum.On },
                new RouteEntity(panel) { State = RouteStateEnum.Active },
                new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Light, State = ButtonStateEnum.On },
            ]);

            Add("Mainline", [
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine, TrackStyle = TrackStyleEnum.Normal },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine, TrackStyle = TrackStyleEnum.Terminator },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine, TrackStyle = TrackStyleEnum.Arrow },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine, TrackStyle = TrackStyleEnum.Lines },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine, TrackStyle = TrackStyleEnum.Bridge },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine, TrackStyle = TrackStyleEnum.Platform },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine, TrackStyle = TrackStyleEnum.Rounded },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine, TrackStyle = TrackStyleEnum.Tunnel },
                new LeftTurnoutEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new RightTurnoutEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new CrossingEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new AngleCrossingEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new CornerEntity(panel) { TrackType = TrackTypeEnum.MainLine },
            ]);

            Add("Branch", [
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine, TrackStyle = TrackStyleEnum.Normal },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine, TrackStyle = TrackStyleEnum.Terminator },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine, TrackStyle = TrackStyleEnum.Arrow },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine, TrackStyle = TrackStyleEnum.Lines },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine, TrackStyle = TrackStyleEnum.Bridge },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine, TrackStyle = TrackStyleEnum.Platform },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine, TrackStyle = TrackStyleEnum.Rounded },
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine, TrackStyle = TrackStyleEnum.Tunnel },
                new LeftTurnoutEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new RightTurnoutEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new CrossingEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new AngleCrossingEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new CornerEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
            ]);
            
            Add("Shapes", [
                new TextEntity(panel),
                new CircleLabelEntity(panel),
                new RectangleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black },
                new LineEntity(panel) { Height = 1, Width = 1, LineColor = Colors.Black, LineWidth = 4 },
                new CircleEntity(panel) { BackgroundColor = Colors.Silver, BorderColor = Colors.Black },
                new ImageEntity(panel),
            ]);
            
            Add("Special", [
                new FastClockEntity(panel) { FastclockType = FastClockTypeEnum.Analog },
                new ScriptButtonEntity(panel) { State = ButtonStateEnum.On },
                new CompassEntity(panel) { },
            ]);

            return palette;

            // Add function to add to the collection
            // -----------------------------------------------------
            void Add(string category, IEnumerable<Entity> entities) {
                var group = new PaletteGroup(category);
                foreach (var e in entities) {
                    var tile = TileFactory.CreateTile(e, 32, false);
                    if (tile is { }) {
                        group.AddTile(tile);
                    }
                }
                palette.Add(group);
            }
        } catch (Exception ex) {
            throw new ApplicationException($"Error building palette: {ex.Message}", ex);
        }
    }
}

