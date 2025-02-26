using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class TurnoutsEditView : ContentPage {
    public TurnoutsEditViewModel? ViewModel;

    public TurnoutsEditView(Turnout turnout) {
        InitializeComponent();

        //ViewModel = new TurnoutsEditViewModel(turnout);
        ViewModel = MauiProgram.ServiceHelper.GetService<TurnoutsEditViewModel>();
        BindingContext = ViewModel;
    }
}