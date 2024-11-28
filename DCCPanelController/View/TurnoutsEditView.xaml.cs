using CommunityToolkit.Maui.Views;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class TurnoutsEditView : Popup {
    
    private TurnoutsEditViewModel? _viewModel;
    
    public TurnoutsEditView(Turnout turnout) {
        InitializeComponent();
        _viewModel = new TurnoutsEditViewModel(turnout);
        _viewModel.CloseRequested += (sender, args) => Close(sender);
        BindingContext = _viewModel;
    }
}