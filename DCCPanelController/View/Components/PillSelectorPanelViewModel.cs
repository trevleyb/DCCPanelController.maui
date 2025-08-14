using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.Helpers;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using Syncfusion.Maui.Toolkit.SegmentedControl;

namespace DCCPanelController.View.TileSelectors;

public partial class PillSelectorPanelViewModel : Base.BaseViewModel {
    private readonly Panels _symbolPanels;

    [ObservableProperty] 
    private string _selectedCategory = string.Empty;

    // Category → tiles
    public ObservableCollection<SfSegmentItem> Categories { get; } = [];
    private readonly Dictionary<string, ObservableCollection<ITile>> _byCategory = new();

    public ObservableCollection<ITile> TilesForSelectedCategory =>
        string.IsNullOrEmpty(SelectedCategory) || !_byCategory.ContainsKey(SelectedCategory)
            ? new ObservableCollection<ITile>()
            : _byCategory[SelectedCategory];

    public PillSelectorPanelViewModel() {
        _symbolPanels = new Panels();
        BuildAllTiles();
        if (Categories.Count > 0) SelectedCategory = Categories[0].Text;
    }

    private void BuildAllTiles(Panel? source = null) {
        using (new CodeTimer("BuildPillTiles", false)) {
            Categories.Clear();
            _byCategory.Clear();

            var panel = source == null ? _symbolPanels.CreatePanel() : _symbolPanels.CreatePanelFrom(source);

            // ——— Build categories/tiles to match Swift defaults ———
            // Swift categories: Actions, Track, Drawing (plus you had Branch & Shapes in the side panel)
            // (We’ll keep your existing groupings, since you already rely on them.)
            Add("Actions", [
                new TurnoutButtonEntity(panel) { State = ButtonStateEnum.On },
                new ActionButtonEntity(panel) { State = ButtonStateEnum.On },
                new RouteEntity(panel) { State = RouteStateEnum.Active },
                new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Light, State = ButtonStateEnum.On },
                new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Switch, State = ButtonStateEnum.On },
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
                new PlatformEntity(panel) { TrackType = TrackTypeEnum.MainLine }
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
                new LineEntity(panel) { Height = 1, Width = 1, LineColor = Colors.Black, LineWidth = 3 },
                new CircleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black },
                new ImageEntity(panel),
            ]);

            // Refresh currently shown tiles
            OnPropertyChanged(nameof(TilesForSelectedCategory));
        }

        // Local helper to add a category
        void Add(string category, IEnumerable<Entity> entities) {
            var list = new ObservableCollection<ITile>();
            foreach (var e in entities) {
                var tile = TileFactory.CreateTile(e, 32, TileDisplayMode.Symbol);
                if (tile is not null) list.Add(tile);
            }
            _byCategory[category] = list;
            
            // Add the item as a Segment Item
            // ---------------------------------------------
            var item = new SfSegmentItem {
                Text = category,
                SelectedSegmentBackground = new SolidColorBrush(StyleHelper.FromStyle("Primary")),
            };
            Categories.Add(item);
        }
    }

    partial void OnSelectedCategoryChanged(string value) => OnPropertyChanged(nameof(TilesForSelectedCategory));
}