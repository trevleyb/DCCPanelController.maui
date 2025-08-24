using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class TestPage : ContentPage {
    public TestPage(TestPageViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
        
        PillSelectorContainer.Panel = viewModel.Panel;
        SideSelectorContainer.Panel = viewModel.Panel;
    }

    private void PillSelectorContainer_OnOnDockSideChanged(object? sender, TileSelectorDockSide e) {
        PillSelectorContainer.IsVisible = false;
        SideSelectorContainer.IsVisible = true;
    }

    private void SideSelectorContainer_OnOnDockSideChanged(object? sender, TileSelectorDockSide e) {
        PillSelectorContainer.IsVisible = true;
        SideSelectorContainer.IsVisible = false;
    }

    private void TapGestureRecognizer_OnTapped1(object? sender, TappedEventArgs e) {
        Console.WriteLine($"Tapped x1");
    }
    private void TapGestureRecognizer_OnTapped2(object? sender, TappedEventArgs e) {
        Console.WriteLine("Tapped x2");
    }

}