using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class TurnoutsPage : ContentPage {
    public TurnoutsPage() {
        InitializeComponent();
        BindingContext = MauiProgram.ServiceHelper.GetService<TurnoutsViewModel>();

        //On<iOS>().SetUseSafeArea(false);
        //var safeInsets = On<iOS>().SafeAreaInsets();
        //MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);
    }
}