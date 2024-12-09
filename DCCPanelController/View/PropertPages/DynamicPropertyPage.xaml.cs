using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View.PropertPages;

public partial class DynamicPropertyPage : ContentPage, IPropertyPage {

    public event EventHandler? CloseRequested;

    public DynamicPropertyPage(ITrackPiece obj, string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageViewModel(obj, propertyName, PropertyContainer);
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        //Navigation.PopModalAsync(true);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}