using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public class TurnoutActionsGridViewModel : ActionsGridViewModel<TurnoutAction, TurnoutActions> {
    public TurnoutActionsGridViewModel(IActionEntity entity, ActionsContext context, List<string> availableTurnouts) : base(entity, context, availableTurnouts) {
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(PanelActions)) {
                OnPropertyChanged(nameof(TurnoutPanelActions));
            }
        };
    }

    public TurnoutActions TurnoutPanelActions => _entity.TurnoutPanelActions;
    protected override TurnoutActions PanelActions => TurnoutPanelActions;
    protected override string ItemTypeName => "Turnout";

    protected override TurnoutAction CreateNewAction(string id) {
        return new TurnoutAction {
            Id = id,
            WhenClosed = TurnoutStateEnum.Closed,
            WhenThrown = TurnoutStateEnum.Thrown
        };
    }

    protected override string GetActionId(TurnoutAction action) {
        return action.Id;
    }
}

// using System.Collections.ObjectModel;
// using CommunityToolkit.Mvvm.ComponentModel;
// using CommunityToolkit.Mvvm.Input;
// using DCCPanelController.Models.DataModel;
// using DCCPanelController.Models.DataModel.Entities;
// using DCCPanelController.Models.DataModel.Entities.Actions;
// using DCCPanelController.Models.DataModel.Entities.Interfaces;
//
// namespace DCCPanelController.View.Actions;
//
// public partial class TurnoutActionsGridViewModel : ObservableObject {
//     [ObservableProperty] private ObservableCollection<string> _selectableItems = [];
//
//     private ActionsContext _actionContext;
//     private List<string> _availableItems;
//     private readonly IActionEntity _entity;
//     public TurnoutActions TurnoutPanelActions => _entity.TurnoutPanelActions;
//
//     public TurnoutActionsGridViewModel(IActionEntity entity, ActionsContext context, List<string> availableTurnouts) {
//         _entity = entity;
//         _actionContext = context;
//         _availableItems = availableTurnouts;
//         UpdateSelectableItems();
//     }
//
//     public bool IsTurnoutContext => _actionContext == ActionsContext.Turnout;
//     public bool IsButtonContext => _actionContext == ActionsContext.Button;
//     public bool IsGridVisible => TurnoutPanelActions.Count > 0;
//     public bool IsAddButtonEnabled => SelectableItems.Count > 0;
//     public double ControlHeight => 40 + (TurnoutPanelActions.Count * 40);
//
//     public string NoDataText {
//         get {
//             if (_availableItems.Count == 0) return "No available Turnouts defined.";
//             if (TurnoutPanelActions.Count == 0 && IsAddButtonEnabled) return "Use the + key to add a turnout action.";
//             if (SelectableItems.Count == 0) return "All available turnouts have been assigned.";
//             return "Use the + key to add more turnout actions.";
//         }
//     }
//
//     private void RaisePropertiesChanged() {
//         OnPropertyChanged(nameof(TurnoutPanelActions));
//         OnPropertyChanged(nameof(ControlHeight));
//         OnPropertyChanged(nameof(NoDataText));
//         OnPropertyChanged(nameof(IsAddButtonEnabled));
//         OnPropertyChanged(nameof(IsGridVisible));
//         OnPropertyChanged(nameof(IsTurnoutContext));
//         OnPropertyChanged(nameof(IsButtonContext));
//     }
//     
//     [RelayCommand]
//     private void AddRow() {
//         if (SelectableItems.Count > 0) {
//             TurnoutPanelActions.Add(new TurnoutAction { Id = SelectableItems[0], WhenClosed = TurnoutStateEnum.Closed, WhenThrown = TurnoutStateEnum.Thrown});
//         }
//         UpdateSelectableItems();
//     }
//
//     [RelayCommand]
//     private void RemoveRow(TurnoutAction? panelAction) {
//         if (panelAction is not null) {
//             var itemToRemove = TurnoutPanelActions.FirstOrDefault(x => x.Id == panelAction.Id);
//             if (itemToRemove != null) {
//                 TurnoutPanelActions.Remove(itemToRemove);
//             }
//         }
//         UpdateSelectableItems();
//     }
//
//     [RelayCommand]
//     private void IdValueChanged(string id) {
//         RaisePropertiesChanged();    
//     }
//
//     public void UpdateSelectableItems(string? activeItem = null) {
//         foreach (var item in _availableItems) {
//             var used = TurnoutPanelActions.Any(x => x.Id == item);
//             var current = !string.IsNullOrEmpty(activeItem) && activeItem.Equals(item);
//             
//             // if it has already been used but is not the current item we are dealing with, 
//             // and if it is in the list, then remove it from the selectable items list. 
//             // ---------------------------------------------------------------------------------
//             if (used && !current && SelectableItems.Contains(item)) SelectableItems.Remove(item); 
//             if (((used && current) || !used) && !SelectableItems.Contains(item)) SelectableItems.Add(item);
//         }
//         RaisePropertiesChanged();
//     }
// }