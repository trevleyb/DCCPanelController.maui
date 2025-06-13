using DCCClient.Discovery;
using DCCPanelController.Clients;
using DCCPanelController.Services;
using DCCPanelController.View.Settings.Jmri;

namespace DCCPanelController.View.Settings.WiThrottle;

public partial class WiThrottleSettingsView : ContentView {
    
    public WiThrottleSettingsView(IDccClientSettings settings, ConnectionService connectionService) {
        var viewModel = new WiThrottleSettingsViewModel(settings, connectionService);
        BindingContext = viewModel;
        InitializeComponent();
        
        // Propagate any messages from the underlying setting module 
        // so the parent can access these and show them in the UI
        // ---------------------------------------------------------------
        viewModel.PropertyChanged += (sender, args) => OnPropertyChanged(args.PropertyName);
    }
    
    public void OnEntryFocused(object sender, FocusEventArgs e) {
        if (sender is Entry entry) {
            entry.CursorPosition = 0;
            entry.SelectionLength = entry.Text?.Length ?? 0;
        }
    }
    
    private void SelectableItemsView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        var selected = e?.CurrentSelection?.FirstOrDefault() ?? null;
        if (selected is DiscoveredService service && BindingContext is WiThrottleSettingsViewModel viewModel) {
            viewModel.WiThrottleSettings.Address = service?.Address.ToString() ?? "0.0.0.0";
            viewModel.WiThrottleSettings.Port = service?.Port ?? 12080;
        }
    }

}