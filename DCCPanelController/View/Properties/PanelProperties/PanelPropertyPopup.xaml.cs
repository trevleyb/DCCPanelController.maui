using CommunityToolkit.Maui.Views;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.PanelProperties;

public partial class PanelPropertyPopup : Popup {
    
    private readonly TaskCompletionSource<bool> _closeTcs = new TaskCompletionSource<bool>();
    public Task<bool> PageClosed => _closeTcs.Task;

    public PanelPropertyPopup(Panel panel) {
        InitializeComponent();
        var propertyDetails = new PanelPropertyBase(panel);
        Properties.Children.Add(propertyDetails);
        BindingContext = propertyDetails.BindingContext;
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        _closeTcs.TrySetResult(true); // or return data as needed
        Close();
    }
}