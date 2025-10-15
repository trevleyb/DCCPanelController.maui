using System.Net.Mime;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.Services;
using DCCPanelController.View.ControlPanel;
using DCCPanelController.View.TileSelectors;
using Markdig;
using Microsoft.Maui.Layouts;
using Svg.Model.Drawables.Elements;
using Font = Microsoft.Maui.Graphics.Font;

namespace DCCPanelController.View;

public partial class TestPage : ContentPage {
    //private readonly GridGestureHelper _gridGestures;

    public TestPage(TestPageViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        if (width <= 0 || height <= 0) return;
        UpdateScreenDimensions(width, height);
    }

    private void UpdateScreenDimensions(double width, double height) {
        if (height > width) {
            SidePaletteContainer.IsVisible = false;
            BottomPaletteContainer.IsVisible = true;
        } else {
            SidePaletteContainer.IsVisible = true;
            BottomPaletteContainer.IsVisible = false;
        }
    }

    
    private void SideDockClosed(object? sender, EventArgs e) {
        SidePaletteContainer.IsVisible = false;
        BottomPaletteContainer.IsVisible = true;
    }
    private void BottomDockClosed(object? sender, EventArgs e) {
        SidePaletteContainer.IsVisible = true;
        BottomPaletteContainer.IsVisible = false;
    }

}