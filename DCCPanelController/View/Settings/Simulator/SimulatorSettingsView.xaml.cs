using DCCPanelController.Clients;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings.Simulator;

public partial class SimulatorSettingsView : ContentView {
    public SimulatorSettingsView(IDccClientSettings settings, ConnectionService connectionService) {
        var viewModel = new SimulatorSettingsViewModel(settings, connectionService);
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
}