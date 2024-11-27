using CommunityToolkit.Maui.Views;
using DCCPanelController.Model;

namespace DCCPanelController.View;

public partial class TurnoutsEditView : Popup {
    public TurnoutsEditView(Turnout turnout) {
        InitializeComponent();

        // Bind the data to the UI elements
        SystemNameEntry.Text = turnout.Id;
        UserNameEntry.Text = turnout.Name;
        CurrentStateLabel.Text = turnout.State.ToString();
        DefaultStatePicker.SelectedItem = turnout.State;

        // Handle Save and Cancel commands
        SaveCommand = new Command(() => SaveTurnout(turnout));
        CancelCommand = new Command(Close);
        BindingContext = this;
    }

    public Command SaveCommand { get; }
    public Command CancelCommand { get; }

    private void SaveTurnout(Turnout turnout) {
        turnout.Id = SystemNameEntry.Text;
        turnout.Name = UserNameEntry.Text;
        turnout.State = (TurnoutStateEnum)DefaultStatePicker.SelectedItem;
        Close(turnout);
    }
}