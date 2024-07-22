using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage, INotifyPropertyChanged {

    public PanelsPage() {
        InitializeComponent();
        var viewModel = App.ServiceProvider?.GetService<PanelsViewModel>();
        if (viewModel != null) {
            viewModel.Sender = this;
            BindingContext = viewModel;
        }
        this.SizeChanged += (_, __) => UpdateLayout();
    }
    
    void UpdateLayout() {
        var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
        var span = orientation switch {
            DisplayOrientation.Portrait  => OnScreenSizeHelpers.Instance.GetScreenSizeValue(1, 1, 1, 1, 1, 2),
            DisplayOrientation.Landscape => OnScreenSizeHelpers.Instance.GetScreenSizeValue(2, 2, 2, 2, 2, 3),
            _                            => OnScreenSizeHelpers.Instance.GetScreenSizeValue(1, 1, 1, 1, 1, 2),
        };
        PanelsCollectionViewLayout.Span = span;
    }
    
    protected override void OnAppearing() {
        base.OnAppearing();
        //if (BindingContext is PanelsViewModel viewModel) {
        //    viewModel.Save();
        //    
        //}
    }
}