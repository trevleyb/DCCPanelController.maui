using DCCPanelController.Models.DataModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View.PanelProperties;

public partial class PanelPropertyPage : ContentPage {
    
    private readonly TaskCompletionSource<bool> _closeTcs = new TaskCompletionSource<bool>();
    public Task<bool> PageClosed => _closeTcs.Task;

    public PanelPropertyPage(Panel panel) {
        InitializeComponent();
#if IOS
        this.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
#endif
        var propertyDetails = new PanelPropertyBase(panel);
        Properties.Children.Add(propertyDetails);
        BindingContext = propertyDetails.BindingContext;
        Title = panel.Id;
    }

    protected override void OnDisappearing() {
         base.OnDisappearing();
         //_closeTcs.TrySetResult(true); // or return data as needed
    }
    
    private async void ClosePropertyPage(object? sender, EventArgs? e) {
         await Navigation.PopModalAsync(true);
         //_closeTcs.TrySetResult(true);
    }
}