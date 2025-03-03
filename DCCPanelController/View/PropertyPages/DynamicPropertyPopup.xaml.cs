using CommunityToolkit.Maui.Views;
using DCCPanelController.Model.Tracks.Interfaces;
#if IOS
using UIKit;
#endif

namespace DCCPanelController.View.PropertyPages;

/// <summary>
///  Popup Page is used for iPad and MacCatalst
/// </summary>
public partial class DynamicPropertyPopup : Popup {
    private readonly ITrack _track;
    private DynamicPropertyPageViewModel _viewModel;

    public DynamicPropertyPopup(ITrack track, string? propertyName = null) {
        InitializeComponent();
        _track = track;
        _viewModel = new DynamicPropertyPageViewModel(_track, propertyName, PropertyContainer);
        BindingContext = _viewModel;
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        this.Close();
    }
}