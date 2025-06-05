using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Input;
using DccClients.Jmri.Client;
using DCCCommon.Client;
using DCCCommon.Discovery;
using DCCPanelController.Services;

namespace DCCPanelController.View.Settings;

public partial class JmriSettingsView : ContentView {
    public JmriSettingsView(IDccClientSettings settings, ConnectionService connectionService) {
        var viewModel = new Jmri.JmriSettingsViewModel(settings, connectionService);
        BindingContext = viewModel;
        InitializeComponent();
    }
    
    public void OnEntryFocused(object sender, FocusEventArgs e) {
        if (sender is Entry entry) {
            entry.CursorPosition = 0;
            entry.SelectionLength = entry.Text?.Length ?? 0;
        }
    }

}