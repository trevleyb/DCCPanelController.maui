using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ButtonAction> _buttonActions;
    [ObservableProperty] private Dictionary<string, string> _idDescriptions;
    [ObservableProperty] private IList<ButtonStateEnum> _buttonStates;
    
    public ButtonActionsListViewModel(ButtonActions buttonActions, ITrackPiece trackPiece) {
        ButtonActions = buttonActions;
        IdDescriptions = new Dictionary<string,string> {
            { "BTN001", "Start Engine" },
            { "BTN002", "Stop Engine" },
        };
        ButtonStates = Enum.GetValues<ButtonStateEnum>().Cast<ButtonStateEnum>().ToList();
        this.PropertyChanged += (sender, args) => { Console.WriteLine("Property Changed:" + args.PropertyName); };
    }

    public bool IsAddButtonEnabled => ButtonIds.Count > 0;

    // Fix this.....
    public ObservableCollection<string> ButtonIds {
        get {
            var result = new ObservableCollection<string>(
                IdDescriptions.Keys
                              .Where(key => ButtonActions.All(button => button.Id != key)));
            return result;
        }
    }

    public string NoDataText {
        get {
            if (ButtonIds.Count == 0) return "No Buttons are defined so none can be added.";
            if (ButtonActions.Count == 0) return "Use the + key to add a button action.";
            return "";
        }
    }
    
    [RelayCommand]
    private void AddRow() {
        ButtonActions.Add(new ButtonAction());
        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
    }

    [RelayCommand]
    private void RemoveRow(ButtonAction action) {
        ButtonActions.Remove(action);
        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
    }

    [RelayCommand]
    private void IdValueChanged(string id) {    
        Console.WriteLine("ID Value Changed: " + id);
        OnPropertyChanged(nameof(IsAddButtonEnabled));
    }
}