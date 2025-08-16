using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCCPanelController.View;

public partial class PanelSurfaceEditor : ContentPage {
    public PanelSurfaceEditor() {
        InitializeComponent();
        BindingContext = new PanelSurfaceEditorViewModel();
    }

    private void PanelView_OnTileChanged(object? sender, EntitySelectedEventArgs e) {
        Console.WriteLine("Tile Changed");
    }

    private void PanelView_OnTileSelected(object? sender, EntitySelectedEventArgs e) {
        Console.WriteLine("Tile Selected");
    }

    private void PanelView_OnTileTapped(object? sender, EntityTappedEventArgs e) {
        Console.WriteLine("Tile Tapped");
    }
}