using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGridViewModel : ObservableObject {

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ControlHeight))]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGridVisible))]
    private ObservableCollection<ButtonAction> _buttonActions;

    [ObservableProperty]
    private ObservableCollection<string> _selectableButtons;

    [ObservableProperty]
    private ObservableCollection<string> _availableButtons;

    public bool IsGridVisible => ButtonActions.Count > 0;
    public bool IsAddButtonEnabled => SelectableButtons.Count > 0;
    public double ControlHeight => 35 + (ButtonActions.Count * 35);

    public ButtonActionsGridViewModel(ButtonActions buttonActions, ITrackPiece trackPiece) {
        AvailableButtons = FindAvailableButtons(trackPiece);
        SelectableButtons = new ObservableCollection<string>(AvailableButtons);
        ButtonActions = buttonActions;
        UpdateSelectableButtons();
        OnPropertyChanged(nameof(AvailableButtons));
        OnPropertyChanged(nameof(SelectableButtons));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        PropertyChanged += (sender, args) => { Console.WriteLine("Property Changed:" + args.PropertyName); };
    }

    public string NoDataText {
        get {
            if (AvailableButtons.Count == 0) return "No Buttons are defined so none can be added.";
            if (ButtonActions.Count == 0) return "Use the + key to add a button action.";
            if (SelectableButtons.Count == 0) return "All buttons are in use.";
            return "";
        }
    }

    [RelayCommand]
    private void AddRow() {
        if (AvailableButtons.Count > 0) {
            ButtonActions.Add(new ButtonAction() { Id = AvailableButtons[0], WhenActiveOrClosed = ButtonStateEnum.Active, WhenInactiveOrThrown = ButtonStateEnum.Inactive, Cascade = false });
        }
        UpdateSelectableButtons();
        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(ControlHeight));
    }

    [RelayCommand]
    private void RemoveRow(ButtonAction action) {
        ButtonActions.Remove(action);
        UpdateSelectableButtons();
        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(ControlHeight));
    }

    [RelayCommand]
    private void IdValueChanged(string id) {
        UpdateSelectableButtons();
        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(ControlHeight));
        Console.WriteLine("ID Value Changed: " + id);
    }

    /// <summary>
    /// using the current track piece, look at the parent panel collection and
    /// iterate over ALL track pieces. If any are a button, and if the name of that
    /// button is different from the current button, then add that button name
    /// to a collection of available buttons. 
    /// </summary>
    private ObservableCollection<string> FindAvailableButtons(ITrackPiece trackPiece) {
        var foundButtons = new ObservableCollection<string>();
        var thisButton = trackPiece as ITrackButton ;
        if (trackPiece is { Parent: { Tracks: { } tracks } }) {
            foreach (var track in tracks) {
                if (track is ITrackButton trackButton) {
                    if (thisButton != null && thisButton.ButtonID == trackButton.ButtonID) continue;
                    foundButtons.Add(trackButton.ButtonID);
                }
            }
        }
        return foundButtons;
    }

    private void UpdateSelectableButtons() {
        Console.WriteLine($"Updating Selectable Buttons: {string.Join(",", SelectableButtons)}");
        for (var i = AvailableButtons.Count - 1; i >= 0; i--) {
            var button = AvailableButtons[i];

            // If we have already used this button, then remove it from the Selectable ones
            // ---------------------------------------------------------------------------
            var found = ButtonActions.Any(btn => btn.Id == button);
            if (ButtonActions.Any(btn => btn.Id == button)) {
                SelectableButtons.Remove(button);
            } else {
                // Otherwise add it to the Selectable ones as it may have been removed
                // -------------------------------------------------------------------
                if (!SelectableButtons.Contains(button)) {
                    SelectableButtons.Add(button);
                }
            }
        }

        Console.WriteLine($"Updated Selectable Buttons: {string.Join(",", SelectableButtons)}");
    }
}