using DCCClients.Jmri.JMRI.DataBlocks;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class TurnoutsEditView : ContentPage {
    public TurnoutsEditViewModel? ViewModel;

    public TurnoutsEditView(Turnout turnout, ConnectionService connectionService) {
        InitializeComponent();
        ViewModel = new TurnoutsEditViewModel(turnout, connectionService);
        BindingContext = ViewModel;
    }
}