using DCCPanelController.Models.DataModel;
#if IOS
using UIKit;
using UIModalPresentationStyle = UIKit.UIModalPresentationStyle;
#endif

namespace DCCPanelController.View.PanelProperties;

public partial class PanelPropertyPage : ContentPage {
    
    private readonly TaskCompletionSource<bool> _closeTcs = new TaskCompletionSource<bool>();
    public Task<bool> PageClosed => _closeTcs.Task;

    public PanelPropertyPage(Panel panel) {
        InitializeComponent();
        var propertyDetails = new PanelPropertyBase(panel);
        Properties.Children.Add(propertyDetails);
        BindingContext = propertyDetails.BindingContext;
        Title = panel.Id;
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
        _closeTcs.TrySetResult(true); // or return data as needed
    }
    
    // private void ClosePropertyPage(object? sender, EventArgs? e) {
    //     Navigation.PopAsync(true);
    // }
}