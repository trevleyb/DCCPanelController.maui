using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage, INotifyPropertyChanged {
    
    public PanelsPage() {
        InitializeComponent();
        var viewModel = App.ServiceProvider?.GetService<PanelsViewModel>();
        if (viewModel != null) {
            viewModel.Sender = this;
            BindingContext = viewModel;
        }
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        SizeChanged += OnSizeChanged;
    }
    
    protected override void OnDisappearing() {
        base.OnDisappearing();
        SizeChanged -= OnSizeChanged;
    }
    
    private void OnSizeChanged(object? sender, EventArgs e) {
        if (Width > 0 && Height > 0) {
            //if (BindingContext is PanelsViewModel viewMode) viewMode.SetCardHeight(Width, Height);

            if (PanelsCollectionView.ItemsLayout is GridItemsLayout { } layout) {
                if (Width > Height) {
                    layout.Span = 2;
                } else {
                    layout.Span = 1;
                }
            }
        }
    }
}