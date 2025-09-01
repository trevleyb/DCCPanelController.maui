using DCCClient.Discovery;
using DCCPanelController.Clients;
using DCCPanelController.Services;

namespace DCCPanelController.Views.Settings.Jmri;

public partial class JmriSettingsView : ContentView {
    public JmriSettingsView(IDccClientSettings settings, ConnectionService connectionService) {
        var viewModel = new JmriSettingsViewModel(settings, connectionService);
        BindingContext = viewModel;
        InitializeComponent();
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
            viewModel.Address = service?.Address.ToString() ?? "0.0.0.0";
            viewModel.Port = service?.Port ?? 12080;
        }
    }
}