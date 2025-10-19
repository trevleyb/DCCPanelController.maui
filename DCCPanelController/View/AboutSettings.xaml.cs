using DCCPanelController.Helpers;
using DCCPanelController.View.Helpers;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.View;

public partial class AboutSettings : ContentPage {
    public AboutSettings() {
        BindingContext = this;
        VersionString = VersionInfo.Version;
        InitializeComponent();
    }
    public string VersionString { get; set; }
}