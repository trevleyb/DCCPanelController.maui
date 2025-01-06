using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsView : ContentView
{
    public ObservableCollection<TurnoutAction> TurnoutActions { get; init; }
    private bool IsTurnoutContext { get; set; }

    // Dynamic Column Header Labels
    public string StateLabelClosed { get; private set; } = "Closed";
    public string StateLabelThrown { get; private set; } = "Thrown";

    public TurnoutActionsView(TurnoutActions actions, bool isTurnoutContext = true) {
        InitializeComponent();
        TurnoutActions = actions;
        IsTurnoutContext = isTurnoutContext;
        BindingContext = this;
        UpdateLabels();
    }

    private void UpdateLabels() {
        StateLabelClosed = IsTurnoutContext ? "Closed" : "Active";
        StateLabelThrown = IsTurnoutContext ? "Thrown" : "Inactive";
        OnPropertyChanged(nameof(StateLabelClosed));
        OnPropertyChanged(nameof(StateLabelThrown));
    }

    [RelayCommand]
    private void AddTurnoutAction() {
        TurnoutActions.Add(new TurnoutAction { });
        OnPropertyChanged(nameof(TurnoutActions));
    }

    [RelayCommand]
    private void EditTurnoutAction(TurnoutAction selectedTurnout) {
        if (selectedTurnout != null) {
            // Edit logic here
        }
        OnPropertyChanged(nameof(TurnoutActions));
    }

    [RelayCommand]
    private void RemoveTurnoutAction(TurnoutAction selectedTurnout) {
        if (selectedTurnout != null) {
            TurnoutActions.Remove(selectedTurnout);
        }
        OnPropertyChanged(nameof(TurnoutActions));
    }
}