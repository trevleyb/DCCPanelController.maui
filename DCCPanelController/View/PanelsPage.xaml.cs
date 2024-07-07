using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage {
    
    public PanelsPage() {
        InitializeComponent();

        //var service = App.ServiceProvider?.GetService<SettingsService>();
        var viewModel = App.ServiceProvider?.GetService<PanelsViewModel>();
        BindingContext = viewModel;
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, EventArgs e) {
        // Adjust the Span based on orientation
        if (Width > Height) {
            // Landscape mode
            if (PanelsCollectionView.ItemsLayout is GridItemsLayout { } landscape) {
                landscape.Span = 2;
            }
        } else {
            if (PanelsCollectionView.ItemsLayout is GridItemsLayout { } portrait) {
                portrait.Span = 1;
            }
        }
    }
}