using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View;

public partial class VerticalTileSelectorViewModel : BaseViewModel {
    
    [ObservableProperty] private double _gridSize = 32; 
    [ObservableProperty] private ObservableCollection<Tile> _tiles = new();

    public VerticalTileSelectorViewModel() {
        BuildTileList();
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
        var tile = TileFactory.CreateTile(entity, GridSize);
        if (tile is Tile view) {
            Tiles.Add(view);
        } else {
            Console.WriteLine($"Unable to create tile: {tile?.Entity.Name ?? "Unknown"}");
        }
    }
}