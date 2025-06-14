using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View.TileSelectors;

public partial class TileSelectorViewModel : Base.BaseViewModel {
    private readonly Panels _symbolPanels;
    [ObservableProperty] private double _gridSize = 32;
    
    [ObservableProperty] private ObservableCollection<Tile> _actions = new();
    [ObservableProperty] private ObservableCollection<Tile> _tracks = new();
    [ObservableProperty] private ObservableCollection<Tile> _shapes = new();

    public TileSelectorViewModel() {
        _symbolPanels = new Panels();
        BuildTileList();
    }

    public async void BuildTileList(Panel? source = null) {
        using (new CodeTimer("BuildTileList")) {
            Actions.Clear();
            Tracks.Clear();
            Shapes.Clear();
            
            var panel = source == null ? _symbolPanels.CreatePanel() : _symbolPanels.CreatePanelFrom(source);
            AddTile(Actions, new ButtonEntity(panel));
            AddTile(Actions, new RouteEntity(panel));
            AddTile(Actions, new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Light, State = ButtonStateEnum.On} );
            AddTile(Actions, new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Switch, State = ButtonStateEnum.On});
            AddTile(Tracks, new StraightEntity(panel));
            AddTile(Tracks, new StraightContinuationEntity(panel));
            AddTile(Tracks, new PlatformEntity(panel));
            AddTile(Tracks, new CornerEntity(panel));
            AddTile(Tracks, new CornerContinuationEntity(panel));
            AddTile(Tracks, new LeftTurnoutEntity(panel));
            AddTile(Tracks, new RightTurnoutEntity(panel));
            AddTile(Tracks, new TerminatorEntity(panel));
            AddTile(Tracks, new TunnelEntity(panel));
            AddTile(Tracks, new CrossingEntity(panel));
            AddTile(Shapes, new CircleLabelEntity(panel));
            AddTile(Shapes, new TextEntity(panel));
            AddTile(Shapes, new RectangleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black });
            AddTile(Shapes, new LineEntity(panel) { Height = 1, Width = 1, LineColor = Colors.Black, LineWidth = 3 });
            AddTile(Shapes, new CircleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black });
            AddTile(Shapes, new ImageEntity(panel));
        }
    }

    private void AddTile(ObservableCollection<Tile> collection, Entity entity) {
        var tile = TileFactory.CreateTile(entity, GridSize, TileDisplayMode.Symbol);
        if (tile is Tile view) {
            collection.Add(view);
        }
    }
}