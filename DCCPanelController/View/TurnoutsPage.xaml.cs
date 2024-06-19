
namespace DCCPanelController.View;

public partial class TurnoutsPage : ContentPage {

    //public Turnouts? Turnouts { get; set; }
    public TurnoutsPage() {
        InitializeComponent();
        //Turnouts = MauiProgram.ServiceProvider.GetService<Turnouts>();
        BindingContext = this;
    }
}