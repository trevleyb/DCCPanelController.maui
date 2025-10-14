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

    public static Palette Palette { get; private set; } = new Palette();
    
    // Store Lazy<PaletteResult> to guarantee single-construction per key
    private static readonly ConcurrentDictionary<string, PaletteResult> Cache = new();

    // Prebuild (or return existing) without allocating a Panel unless needed
    public static PaletteResult PrebuildDefaultPalette() => GetPalette(DefaultPalette, CreateDefaultPanel());

    public static PaletteResult GetDefaultPalette() => GetPalette(DefaultPalette, CreateDefaultPanel());

    public static PaletteResult GetPalette(Panel panel) => GetPalette(panel.Guid.ToString(), panel.CloneEmptyPanel("Selector"));
    
    public static void Clear(string key) => Cache.TryRemove(key, out _);

    public static void Clear() => Cache.Clear();

    private static PaletteResult GetPalette(string key, Panel panel) {
        var result = BuildTilesForPanel(panel);
        return result ?? throw new InvalidOperationException("Unable to build palette.");
    }

    // --- Implementation details ----------------------------------------------

    private static Panel CreateDefaultPanel() {
        var panels = new Panels();
        return panels.CreatePanel("Palette");
    }

    private static PaletteResult BuildTilesForPanel(Panel panel) {
        try {
            Palette.Clear();
            var byCategory = new Dictionary<string, ObservableCollection<ITile>>(StringComparer.Ordinal);
            var categories = new ObservableCollection<string>();
            
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

            // Return read-only views to prevent accidental mutation of the cache
            var roByCategory = byCategory.ToDictionary(
                kvp => kvp.Key, ObservableCollection<ITile> (kvp) => kvp.Value,
                StringComparer.Ordinal);

            return new PaletteResult(roByCategory, categories);

            // Add function to add to the collection
            // -----------------------------------------------------
            void Add(string category, IEnumerable<Entity> entities) {
                var group = new PaletteGroup(category);
                categories.Add(category);
                var list = new ObservableCollection<ITile>();
                foreach (var e in entities) {
                    var tile = TileFactory.CreateTile(e, 32, false);
                    if (tile is { }) {
                        list.Add(tile);
                        group.AddTile(tile);
                    }
                }
                byCategory[category] = list;
                Palette.Add(group);
            }
        } catch (Exception ex) {
            throw new ApplicationException($"Error building palette: {ex.Message}", ex);
        }
    }

    // Immutable-ish result (lists are read-only)
    public sealed record PaletteResult(
        Dictionary<string, ObservableCollection<ITile>> ByCategory,
        ObservableCollection<string> Categories);
}

public sealed class Palette : ObservableCollection<PaletteGroup>, INotifyPropertyChanged { }

public sealed class PaletteGroup : ObservableCollection<PaletteItem>, INotifyPropertyChanged
{
    private readonly List<PaletteItem> _allItems = new();
    public string Category { get; }
    public bool IsExpanded { get; private set; }

    public void AddTile(ITile tile)
    {
        var item = new PaletteItem(tile, this);
        _allItems.Add(item);
        Add(item);
    }

    public PaletteGroup(string category)
    {
        Category = category;
        IsExpanded = true;
        ToggleExpandCommand = new RelayCommand(ToggleExpanded);
    }

    public ICommand ToggleExpandCommand { get; }

    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsExpanded)));

        if (IsExpanded)
        {
            foreach (var item in _allItems)
            {
                Add(item);
            }
        }
        else
        {
            Clear();
        }
    }
}

public sealed partial class PaletteItem : ObservableObject
{
    public PaletteItem(ITile tile, PaletteGroup group)
    {
        Tile = tile;
        IsSelected = false;
    }

    [ObservableProperty] private ITile _tile;
    [ObservableProperty] private bool _isSelected;
}
