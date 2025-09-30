using DCCPanelController.Clients;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings.Simulator;

public partial class SimulatorSettingsView : ContentView {

    public SimulatorSettingsView(IDccClientSettings settings, ConnectionService connectionService) : this(new SimulatorSettingsViewModel(settings, connectionService)) { }
    public SimulatorSettingsView(SimulatorSettingsViewModel viewModel) {
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