using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGridViewModel : ActionsGridViewModel<ButtonAction, ButtonActions>
{
    public ButtonActions ButtonPanelActions => _entity.ButtonPanelActions;
    protected override ButtonActions PanelActions => ButtonPanelActions;
    protected override string ItemTypeName => "Button";

    public ButtonActionsGridViewModel(IActionEntity entity, ActionsContext context, List<string> availableButtons) : base(entity, context, availableButtons) {
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(PanelActions)) {
                OnPropertyChanged(nameof(ButtonPanelActions));
            }
        };        
    }

    protected override ButtonAction CreateNewAction(string id) {
        return new ButtonAction { 
            Id = id, 
            WhenOn = ButtonStateEnum.On, 
            WhenOff = ButtonStateEnum.Off 
        };
    }

    protected override string GetActionId(ButtonAction action) {
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
// public partial class ButtonActionsGridViewModel : ObservableObject {
//     [ObservableProperty] private ObservableCollection<string> _selectableItems = [];
//
//     private ActionsContext _actionContext;
//     private List<string> _availableItems;
//     private IActionEntity _entity;
//     public ButtonActions ButtonPanelActions => _entity.ButtonPanelActions;
//
//     public ButtonActionsGridViewModel(IActionEntity entity, ActionsContext context, List<string> availableButtons) {
//         _entity = entity;
//         _actionContext = context;
//         _availableItems = availableButtons;
//         UpdateSelectableItems();
//     }
//
//     public bool IsTurnoutContext    => _actionContext == ActionsContext.Turnout;
//     public bool IsButtonContext     => _actionContext == ActionsContext.Button;
//     public bool IsGridVisible       => ButtonPanelActions.Count > 0;
//     public bool IsAddButtonEnabled  => SelectableItems.Count > 0;
//     public double ControlHeight     => 40 + (ButtonPanelActions.Count * 40);
//
//     public string NoDataText {
//         get {
//             if (_availableItems.Count == 0) return "No available Buttons defined. ";
//             if (ButtonPanelActions.Count == 0) return "Use the + key to add a button action.";
//             if (SelectableItems.Count == 0) return "All available buttons have been assigned.";
//             return "Use the + key to add more button actions.";
//         }
//     }
//
//     private void RaisePropertiesChanged() {
//         OnPropertyChanged(nameof(ButtonPanelActions));
//         OnPropertyChanged(nameof(IsTurnoutContext));
//         OnPropertyChanged(nameof(IsButtonContext));
//         OnPropertyChanged(nameof(ControlHeight));
//         OnPropertyChanged(nameof(IsGridVisible));
//         OnPropertyChanged(nameof(IsAddButtonEnabled));
//         OnPropertyChanged(nameof(NoDataText));
//     }
//     
//     [RelayCommand]
//     private void AddRow() {
//         if (SelectableItems.Count > 0) {
//             ButtonPanelActions.Add(new ButtonAction { Id = SelectableItems[0], WhenOn = ButtonStateEnum.On, WhenOff = ButtonStateEnum.Off });
//         }
//         UpdateSelectableItems();
//     }
//
//     [RelayCommand]
//     private void RemoveRow(ButtonAction? buttonAction) {
//         if (buttonAction is not null) {
//             var itemToRemove = ButtonPanelActions.FirstOrDefault(x => x.Id == buttonAction.Id);
//             if (itemToRemove != null) {
//                 ButtonPanelActions.Remove(itemToRemove);
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
//             var used = ButtonPanelActions.Any(x => x.Id == item);
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