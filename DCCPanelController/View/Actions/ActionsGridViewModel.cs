using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public interface IActionsGridViewModel {
    List<string> GetSelectableItems(string? activeItem = null);
}

public abstract partial class ActionsGridViewModel<TAction, TCollection> : ObservableObject, IActionsGridViewModel
    where TAction : class, new()
    where TCollection : ICollection<TAction> {

    protected abstract TCollection PanelActions { get; }
    protected abstract string ItemTypeName { get; }

    protected ActionsContext ActionContext;
    protected List<string>   AvailableItems;
    protected Action?        ChangedAction;

    [ObservableProperty] private   List<string>  _selectableItems = [];

    public ListView? ActionsGridListView;
    
    protected ActionsGridViewModel(ActionsContext context, List<string> availableItems, Action? changedAction) {
        ActionContext = context;
        AvailableItems = availableItems;
        ChangedAction = changedAction;
    }

    public bool IsTurnoutContext => ActionContext == ActionsContext.Turnout;
    public bool IsButtonContext => ActionContext == ActionsContext.Button;
    public bool IsGridVisible => PanelActions?.Count > 0;
    public bool IsAddButtonEnabled => SelectableItems?.Count > 0;
    public double ControlHeight => PanelActions?.Count ?? 1 * 40;

    public string NoDataText {
        get {
            if (AvailableItems?.Count == 0) return$"No available {ItemTypeName}s defined.";
            if (PanelActions?.Count == 0 && IsAddButtonEnabled) return$"Add {ItemTypeName.ToLower()} actions.";
            if (SelectableItems?.Count == 0) return$"All available {ItemTypeName.ToLower()}s are assigned.";
            return$"Use the + key to add more {ItemTypeName.ToLower()} actions.";
        }
    }

    public List<string> GetSelectableItems(string? activeItem = null) {
        UpdateSelectableItems(activeItem);
        return SelectableItems;
    }

    protected abstract TAction CreateNewAction(string id);
    protected abstract string GetActionId(TAction action);

    /// <summary>
    ///     Removes any actions from PanelActions where the Id doesn't exist in AvailableItems.
    ///     This ensures data integrity when available items change (e.g., items deleted elsewhere).
    /// </summary>
    protected void CleanupInvalidActions() {
        // Work backwards through the collection to avoid index issues when removing items
        var actionsToRemove = new List<TAction>();

        foreach (var action in PanelActions) {
            var actionId = GetActionId(action);
            if (!string.IsNullOrEmpty(actionId) && !AvailableItems.Contains(actionId)) {
                actionsToRemove.Add(action);
            }
        }

        // Remove invalid actions
        foreach (var actionToRemove in actionsToRemove) {
            PanelActions.Remove(actionToRemove);
        }

        // If we removed any actions, raise property change notifications
        if (actionsToRemove.Count > 0) {
            RaisePropertiesChanged();
        }
    }

    protected void RaisePropertiesChanged() {
        OnPropertyChanged(nameof(PanelActions));
        OnPropertyChanged(nameof(NoDataText));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
        OnPropertyChanged(nameof(ControlHeight));
    }

    [RelayCommand]
    private void AddRow() {
        if (SelectableItems.Count > 0) {
            var newAction = CreateNewAction(SelectableItems[0]);
            PanelActions.Add(newAction);
        }
        ChangedAction?.Invoke();
        UpdateSelectableItems();
        RaisePropertiesChanged();
        ActionsGridListView?.InvalidateMeasure();
    }

    [RelayCommand]
    private void RemoveRow(TAction? action) {
        if (action is { }) {
            var itemToRemove = PanelActions.FirstOrDefault(x => GetActionId(x) == GetActionId(action));
            if (itemToRemove != null) {
                PanelActions.Remove(itemToRemove);
            }
        }
        ChangedAction?.Invoke();
        UpdateSelectableItems();
        RaisePropertiesChanged();
    }

    [RelayCommand]
    private void IdValueChanged(string id) {
        ChangedAction?.Invoke();
        RaisePropertiesChanged();
    }

    public void UpdateSelectableItems(string? activeItem = null) {
        foreach (var item in AvailableItems) {
            var used = PanelActions.Any(x => GetActionId(x) == item);
            var current = !string.IsNullOrEmpty(activeItem) && activeItem.Equals(item);

            if (used && !current && SelectableItems.Contains(item)) SelectableItems.Remove(item);
            if ((used && current || !used) && !SelectableItems.Contains(item)) SelectableItems.Add(item);
        }
    }
}