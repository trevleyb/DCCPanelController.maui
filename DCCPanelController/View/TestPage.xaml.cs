using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.TileSelectors;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class TestPage : ContentPage {
    private GridGestureHelper _gridGestures;

    public TestPage(TestPageViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
        _gridGestures = new GridGestureHelper(TestGrid);
        _gridGestures.SingleTap += GridGesturesOnSingleTap;
        _gridGestures.DoubleTap += GridGesturesOnDoubleTap;
        _gridGestures.LongPress += GridGesturesOnLongPress;
        _gridGestures.GridSelectionStarted += GridGesturesOnGridSelectionStarted;
        _gridGestures.GridSelectionChanged += GridGesturesOnGridSelectionChanged;
        _gridGestures.GridSelectionCompleted += GridGesturesOnGridSelectionCompleted;
        _gridGestures.GridSelectionCancelled += GridGesturesOnGridSelectionCancelled;
        _gridGestures.TileDragStarted += GridGesturesOnTileDragStarted;
        _gridGestures.TileDragMoved += GridGesturesOnTileDragMoved;
        _gridGestures.TileDragCompleted += GridGesturesOnTileDragCompleted;
        _gridGestures.TileDragCancelled += GridGesturesOnTileDragCancelled;

        // Create a dummy Panel and Add it to the system
        // ---------------------------------------------
        var panels = new Panels();
        var panel  = panels.CreatePanel();
        var entity = panel.CreateEntity<CompassEntity>();
        entity.Col = 2;
        entity.Row = 2;
        var tile = new CompassTile(entity, 15);
        TestGrid.Children.Add(tile);
        TestGrid.SetColumn(tile, tile.Entity.Col);
        TestGrid.SetRow(tile, tile.Entity.Row);
    }

    private void GridGesturesOnTileDragCancelled(object? sender, TileDragEventArgs e) {
        Console.WriteLine($"Tile CANCEL Selection @{e.CurrentCol},{e.CurrentRow} Start @{e.StartCol},{e.StartRow}");
        if (e.Tile is Tile foundTile) {
            foundTile.Entity.Col = e.StartCol;
            foundTile.Entity.Row = e.StartRow;
            TestGrid.SetColumn(foundTile, foundTile.Entity.Col);
            TestGrid.SetRow(foundTile, foundTile.Entity.Row);
        }
    }

    private void GridGesturesOnTileDragCompleted(object? sender, TileDragEventArgs e) {
        Console.WriteLine($"Tile END Selection @{e.CurrentCol},{e.CurrentRow} Start @{e.StartCol},{e.StartRow}");
        if (e.Tile is Tile foundTile) {
            foundTile.Entity.Col = e.CurrentCol;
            foundTile.Entity.Row = e.CurrentRow;
            TestGrid.SetColumn(foundTile, foundTile.Entity.Col);
            TestGrid.SetRow(foundTile, foundTile.Entity.Row);
        }
    }

    private void GridGesturesOnTileDragMoved(object? sender, TileDragEventArgs e) {
        Console.WriteLine($"Tile MOVE Selection @{e.CurrentCol},{e.CurrentRow} Start @{e.StartCol},{e.StartRow}");
        if (e.Tile is Tile foundTile) {
            foundTile.Entity.Col = e.CurrentCol;
            foundTile.Entity.Row = e.CurrentRow;
            TestGrid.SetColumn(foundTile, foundTile.Entity.Col);
            TestGrid.SetRow(foundTile, foundTile.Entity.Row);
        }
    }

    private void GridGesturesOnTileDragStarted(object? sender, TileDragEventArgs e) {
        Console.WriteLine($"Tile START Selection @{e.CurrentCol},{e.CurrentRow} Start @{e.StartCol},{e.StartRow}");
    }
    
    private void GridGesturesOnGridSelectionCancelled(object? sender, GridSelectionEventArgs e) {
        Console.WriteLine($"CANCELLED Selecting from @{e.StartCol},{e.StartRow} to @{e.EndCol},{e.EndRow} and {e.AbsStartCol},{e.AbsStartRow} to {e.AbsEndCol},{e.AbsEndRow}");
    }


    private void GridGesturesOnGridSelectionCompleted(object? sender, GridSelectionEventArgs e) {
        Console.WriteLine($"STOP Selecting from @{e.StartCol},{e.StartRow} to @{e.EndCol},{e.EndRow} and {e.AbsStartCol},{e.AbsStartRow} to {e.AbsEndCol},{e.AbsEndRow}");
    }

    private void GridGesturesOnGridSelectionChanged(object? sender, GridSelectionEventArgs e) {
        Console.WriteLine($"MOVE Selecting from @{e.StartCol},{e.StartRow} to @{e.EndCol},{e.EndRow} and {e.AbsStartCol},{e.AbsStartRow} to {e.AbsEndCol},{e.AbsEndRow}");
    }

    private void GridGesturesOnGridSelectionStarted(object? sender, GridSelectionEventArgs e) {
        Console.WriteLine($"START Selecting from @{e.StartCol},{e.StartRow} to @{e.EndCol},{e.EndRow} and {e.AbsStartCol},{e.AbsStartRow} to {e.AbsEndCol},{e.AbsEndRow}");
    }

    private void GridGesturesOnLongPress(object? sender, GridGestureEventArgs e) {
        Console.WriteLine($"Long Press @{e.Col},{e.Row}");
    }

    private void GridGesturesOnDoubleTap(object? sender, GridGestureEventArgs e) {
        Console.WriteLine($"Double Tap @{e.Col},{e.Row}");
    }

    private void GridGesturesOnSingleTap(object? sender, GridGestureEventArgs e) {
        Console.WriteLine($"Single Tap @{e.Col},{e.Row}");
    }
}