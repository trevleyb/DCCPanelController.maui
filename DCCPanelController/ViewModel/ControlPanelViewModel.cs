using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Tracks.Base;

namespace DCCPanelController.ViewModel;

public partial class ControlPanelViewModel : BaseViewModel {


    [ObservableProperty] private Panel _panel;

    public ControlPanelViewModel() {
        // Load up the View for the given Panel 
    }
}