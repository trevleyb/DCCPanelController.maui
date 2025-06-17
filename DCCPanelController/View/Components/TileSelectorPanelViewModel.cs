
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View.TileSelectors;

public partial class TileSelectorPanelViewModel : Base.BaseViewModel {
    private readonly Panels _symbolPanels;

    private const int BorderMargin = 5;
    private const int IconMargin = 5;
    private const int DefaultSelectorWidth = 72;
    
    [ObservableProperty] private double _borderSize = 32;
    [ObservableProperty] private double _iconSize = 32;
    [ObservableProperty] private double _gridSize = 32;

    [ObservableProperty] private double _selectorWidth = 64; // Default width
    [ObservableProperty] private int _columnCount = 1;
    [ObservableProperty] private double _columnSpacing = 4;
    
    [ObservableProperty] private ObservableCollection<Tile> _actions = new();
    [ObservableProperty] private ObservableCollection<Tile> _tracks = new();
    [ObservableProperty] private ObservableCollection<Tile> _branch = new();
    [ObservableProperty] private ObservableCollection<Tile> _shapes = new();

    public TileSelectorPanelViewModel() {
        _symbolPanels = new Panels();
        BuildTileList();
    }

    public Panel? Panel {
        get;
        set {
            field = value;
            if (field is not null) {
                SelectorWidth = field?.Panels?.Profile?.Settings?.SelectorWidth ?? DefaultSelectorWidth;
                UpdateLayout();
            }
        }
    }
    
    partial void OnSelectorWidthChanged(double value) {
        if (Panel?.Panels?.Profile?.Settings is { } settings) settings.SelectorWidth = SelectorWidth;
        UpdateLayout();
    }

    private void UpdateLayout() {
        const double minWidth = 32;
        const double maxTileSize = 64; // 2x minimum
        
        // Ensure minimum width
        if (SelectorWidth < minWidth) {
            SelectorWidth = minWidth;
            return;
        }

        // Calculate optimal tile size and column count
        double availableWidth = SelectorWidth - ColumnSpacing;
        
        if (availableWidth <= maxTileSize) {
            // Single column, tiles can grow up to maxTileSize
            ColumnCount = 1;
            GridSize = Math.Max(minWidth, availableWidth);
        } else {
            // Multiple columns needed
            int columns = (int)Math.Floor(availableWidth / minWidth);
            
            // Calculate tile size with spacing
            double totalSpacing = (columns - 1) * ColumnSpacing;
            double tileSize = (availableWidth - totalSpacing) / columns;
            
            // Ensure tiles don't get too small
            if (tileSize < minWidth) {
                columns = Math.Max(1, (int)Math.Floor((availableWidth + ColumnSpacing) / (minWidth + ColumnSpacing)));
                totalSpacing = (columns - 1) * ColumnSpacing;
                tileSize = (availableWidth - totalSpacing) / columns;
            }
            
            ColumnCount = columns;
            GridSize = Math.Max(minWidth, tileSize);
        }
        BorderSize = GridSize - BorderMargin;
        IconSize = BorderSize - IconMargin;
    }

    public async void BuildTileList(Panel? source = null) {
        using (new CodeTimer("BuildTileList",false)) {
            Actions.Clear();
            Tracks.Clear();
            Shapes.Clear();
            Branch.Clear();
            
            var panel = source == null ? _symbolPanels.CreatePanel() : _symbolPanels.CreatePanelFrom(source);
            AddTile(Actions, new ButtonEntity(panel) {State = ButtonStateEnum.On }  );
            AddTile(Actions, new RouteEntity(panel) {State = ButtonStateEnum.On } );
            AddTile(Actions, new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Light, State = ButtonStateEnum.On} );
            AddTile(Actions, new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Switch, State = ButtonStateEnum.On});
            
            AddTile(Tracks, new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new StraightContinuationEntity(panel){ TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new PlatformEntity(panel){ TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new CornerEntity(panel){ TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new CornerContinuationEntity(panel){ TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new LeftTurnoutEntity(panel){ TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new RightTurnoutEntity(panel){ TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new TerminatorEntity(panel){ TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new TunnelEntity(panel){ TrackType = TrackTypeEnum.MainLine});
            AddTile(Tracks, new CrossingEntity(panel){ TrackType = TrackTypeEnum.MainLine});

            AddTile(Branch, new StraightEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new StraightContinuationEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new PlatformEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new CornerEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new CornerContinuationEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new LeftTurnoutEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new RightTurnoutEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new TerminatorEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new TunnelEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            AddTile(Branch, new CrossingEntity(panel){ TrackType = TrackTypeEnum.BranchLine});
            
            AddTile(Shapes, new CircleLabelEntity(panel));
            AddTile(Shapes, new TextEntity(panel));
            AddTile(Shapes, new RectangleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black });
            AddTile(Shapes, new LineEntity(panel) { Height = 1, Width = 1, LineColor = Colors.Black, LineWidth = 3 });
            AddTile(Shapes, new CircleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black });
            AddTile(Shapes, new ImageEntity(panel));
        }
    }

    private void AddTile(ObservableCollection<Tile> collection, Entity entity) {
        var tile = TileFactory.CreateTile(entity, IconSize, TileDisplayMode.Symbol);
        if (tile is Tile view) {
            collection.Add(view);
        }
    }
}