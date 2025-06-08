using DCCCommon.Client;
using DCCCommon.Discovery;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings.Jmri;

public partial class JmriSettingsView : ContentView, IRaisesSettingsMessage {
    
    public event EventHandler<SettingsMessage>? OnSettingsMessage;

    public JmriSettingsView(IDccClientSettings settings, ConnectionService connectionService) {
        var viewModel = new JmriSettingsViewModel(settings, connectionService);
        BindingContext = viewModel;
        InitializeComponent();
        
        // Propagate any messages from the underlying setting module 
        // so the parent can access these and show them in the UI
        // ---------------------------------------------------------------
        viewModel.OnSettingsMessage += (sender, message) => OnSettingsMessage?.Invoke(sender, message);
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
        if (selected is DiscoveredService service && BindingContext is JmriSettingsViewModel viewModel) {
            viewModel.JmriSettings.Address = service?.Address.ToString() ?? "0.0.0.0";
            viewModel.JmriSettings.Port = service?.Port ?? 12080;
        }
    }
}