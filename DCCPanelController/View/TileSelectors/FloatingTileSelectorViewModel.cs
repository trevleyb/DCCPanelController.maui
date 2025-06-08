using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View.TileSelectors;

public partial class FloatingTileSelectorViewModel : Base.BaseViewModel {
    [ObservableProperty] private double _gridSize = 32;
    [ObservableProperty] private double _height;
    [ObservableProperty] private double _lastX;
    [ObservableProperty] private double _lastY;
    [ObservableProperty] private string _layoutBounds = "0.5,0.5,autosize,autosize";
    [ObservableProperty] private ObservableCollection<Tile> _tiles = new();
    [ObservableProperty] private double _width;

    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;

    public FloatingTileSelectorViewModel() {
        BuildTileList();
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (Width > 0 && Height > 0) {
            var posX = X / Width;
            var posY = Y / Height;
            LayoutBounds = $"{posX},{posY},autosize,autosize";
            Console.WriteLine($"Bounds={LayoutBounds}");
        }
    }

    private void BuildTileList() {
        var tilesPanels = new Panels();
        var panel = tilesPanels.CreatePanel();
        AddTile(new ButtonEntity(panel));
        AddTile(new CompassEntity(panel));
        AddTile(new CornerEntity(panel));
        AddTile(new CornerContinuationEntity(panel));
        AddTile(new CrossingEntity(panel));
        AddTile(new LeftTurnoutEntity(panel));
        AddTile(new RightTurnoutEntity(panel));
        AddTile(new StraightContinuationEntity(panel));
        AddTile(new StraightEntity(panel));
        AddTile(new TerminatorEntity(panel));
        AddTile(new PointsEntity(panel));
        AddTile(new ImageEntity(panel));
        AddTile(new TextEntity(panel));
        AddTile(new RectangleEntity(panel));
        AddTile(new LineEntity(panel));
        AddTile(new CircleEntity(panel));
        AddTile(new CircleLabelEntity(panel));
    }

    private void AddTile(Entity entity) {
        entity.Width = 1;
        entity.Height = 1;
        var tile = TileFactory.CreateTile(entity, GridSize, TileDisplayMode.Symbol);
        if (tile is Tile view) Tiles.Add(view);
    }
}