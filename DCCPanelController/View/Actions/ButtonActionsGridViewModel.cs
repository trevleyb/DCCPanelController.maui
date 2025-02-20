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
    private ObservableCollection<ButtonAction> _buttonActions;

    [ObservableProperty] 
    private ObservableCollection<string> _selectableButtons;
    
    protected readonly List<string> AvailableButtons;
    public bool IsAddButtonEnabled => SelectableButtons.Count > 0;
    public double ControlHeight => 100 + (ButtonActions.Count * 35);
    
    public ButtonActionsGridViewModel(ButtonActions buttonActions, ITrackPiece trackPiece) {
        ButtonActions = buttonActions;
        AvailableButtons = FindAvailableButtons(trackPiece);
        SelectableButtons = new ObservableCollection<string>(AvailableButtons);
        UpdateSelectableButtons();
        PropertyChanged += (sender, args) => { Console.WriteLine("Property Changed:" + args.PropertyName); };
    }
    
    /// <summary>
    /// using the current track piece, look at the parent panel collection and
    /// iterate over ALL track pieces. If any are a button, and if the name of that
    /// button is different from the current button, then add that button name
    /// to a collection of available buttons. 
    /// </summary>
    private List<string> FindAvailableButtons(ITrackPiece trackPiece) {
        List<string> foundButtons = [];
        if (trackPiece is { Parent: { Tracks: { } tracks } }) {
            foreach (var track in tracks) {
                if (track is ITrackButton && !track.Name.Equals(trackPiece.Name)) {
                    foundButtons.Add(track.Name);
                }
            }
        }
        foundButtons.Add("BTN001");
        foundButtons.Add("BTN002");
        return foundButtons;
    }
    
    public string NoDataText {
        get {
            if (AvailableButtons.Count == 0) return "No Buttons are defined so none can be added.";
            if (ButtonActions.Count == 0) return "Use the + key to add a button action.";
            return "";
        }
    }
    
    [RelayCommand]
    private void AddRow() {
        if (AvailableButtons.Count > 0) {
            ButtonActions.Add(new ButtonAction() { Id = AvailableButtons[0], WhenActiveOrClosed = ButtonStateEnum.Active, WhenInactiveOrThrown = ButtonStateEnum.Inactive, Cascade = false});
            UpdateSelectableButtons();
        }
        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(ControlHeight));
    }

    [RelayCommand]
    private void RemoveRow(ButtonAction action) {
        ButtonActions.Remove(action);
        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(ControlHeight));
    }

    [RelayCommand]
    private void IdValueChanged(string id) {    
        Console.WriteLine("ID Value Changed: " + id);
        UpdateSelectableButtons();
        OnPropertyChanged(nameof(IsAddButtonEnabled));
    }

    private void UpdateSelectableButtons() {
        foreach (var button in AvailableButtons) {
            
            // If we have already used this button, then remove it from the Selectable ones
            // ---------------------------------------------------------------------------
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
    }
}