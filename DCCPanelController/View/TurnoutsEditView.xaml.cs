using CommunityToolkit.Maui.Views;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class TurnoutsEditView : ContentPage {
    
    public TurnoutsEditViewModel? ViewModel;

    public TurnoutsEditView(Turnout turnout) {
        InitializeComponent();
        ViewModel = new TurnoutsEditViewModel(turnout);
        //ViewModel.CloseRequested += async (sender, e) => {
        //    if (e is null) {
        //        // Close the page without saving
        //        if (DeviceInfo.Idiom == DeviceIdiom.Phone) {
        //            await Navigation.PopAsync();
        //        } else {
        //            // Close the popup
        //            //await this.clDismiss(null);
        //        }
        //    } else {
        //        // Handle saving logic, if necessary
        //    }
        //};
        BindingContext = ViewModel;
    }
}