using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage, INotifyPropertyChanged {

    public PanelsPage() {
        BindingContext = new PanelsViewModel();
        InitializeComponent();
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
}