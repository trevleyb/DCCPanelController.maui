using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View.TileSelectors;

public partial class VerticalTileSelectorViewModel : BaseViewModel {

    private Panels _symbolPanels;
    [ObservableProperty] private double _gridSize = 32; 
    [ObservableProperty] private ObservableCollection<Tile> _tiles = new();

    public VerticalTileSelectorViewModel() {
        _symbolPanels = new Panels();
        Task.Run(()=>BuildTileList());
    }
    
    public async void BuildTileList(Panel? source = null) {
        using (new CodeTimer("BuildTileList")) {
            Tiles.Clear();
            var panel = (source == null) ? _symbolPanels.CreatePanel() : _symbolPanels.CreatePanelFrom(source);
            
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
            AddTile(new RectangleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black});
            AddTile(new LineEntity(panel) { Height = 1, Width = 1, LineColor = Colors.Black,LineWidth = 3});
            AddTile(new CircleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black});
            AddTile(new CircleLabelEntity(panel));
        }
    }

    private void AddTile(Entity entity) {
        var tile = TileFactory.CreateTile(entity, GridSize, TileDisplayMode.Symbol);
        if (tile is Tile view) {
            Tiles.Add(view);
        } 
    }
}