using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage {
    
    private int _draggingIndex;
    private PanelsViewModel? _viewModel;
    
    public PanelsPage() {
        InitializeComponent();
        //var service = App.ServiceProvider?.GetService<SettingsService>();
        _viewModel =App.ServiceProvider?.GetService<PanelsViewModel>();
        //_viewModel = new PanelsViewModel(service!, this);
        BindingContext = _viewModel;
        this.SizeChanged += OnSizeChanged;
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