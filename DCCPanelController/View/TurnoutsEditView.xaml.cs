using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View;

public partial class TurnoutsEditView : ContentPage {
    public TurnoutsEditViewModel? ViewModel;

    public TurnoutsEditView() { }

    public TurnoutsEditView(Turnout turnout) {
        InitializeComponent();
        ViewModel = MauiProgram.ServiceHelper.GetService<TurnoutsEditViewModel>();
        BindingContext = ViewModel;
    }
}